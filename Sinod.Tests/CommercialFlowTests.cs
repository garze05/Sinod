using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Sinod.Api.Contracts;
using Sinod.Api.Contracts.Clients;
using Sinod.Api.Entities;

namespace Sinod.Tests;

public sealed class CommercialFlowTests(PostgresFixture fixture) : IClassFixture<PostgresFixture>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private HttpClient Client() => fixture.Factory.CreateClient(new() { BaseAddress = new Uri("https://localhost") });

    [Fact]
    public async Task UpgradePreservesAndNumbersLegacyVersionsButDoesNotTreatThemAsCompleteQuotes()
    {
        using var http = Client();
        var opportunity = await Get<OpportunityResponse>(http, "/api/opportunities/-1");
        Assert.Equal(new[] { 1, 2 }, opportunity.Quotations.Select(q => q.Version));
        Assert.Equal(HttpStatusCode.Conflict, (await http.PostAsync("/api/opportunities/-1/quotations/-1/send", null)).StatusCode);
    }

    [Fact]
    public async Task RegularClientNeedsFiftyPercentAndPreservesHistory()
    {
        using var http = Client();
        var setup = await SetupAsync(http);
        var opportunity = await CreateOpportunity(http, setup.Client.Id, setup.Service.Id);
        var path = $"/api/opportunities/{opportunity.Id}";
        var quote = await Post<QuotationResponse>(http, path + "/quotations", Quote(setup.Service.Id));
        Assert.Equal(1, quote.Version);
        Assert.Equal(10000m, quote.Total);
        Assert.Equal(HttpStatusCode.Conflict, (await http.PostAsync(path + $"/quotations/{quote.Id}/accept", null)).StatusCode);
        await Post<QuotationResponse>(http, path + $"/quotations/{quote.Id}/send", null, HttpStatusCode.OK);

        await Put<ServiceResponse>(http, $"/api/services/{setup.Service.Id}", new ServiceRequest
        {
            Name = "Nombre actualizado", PricingProfileId = setup.Profile.Id, BasePrice = 20000
        });
        var second = await Post<QuotationResponse>(http, path + "/quotations", Quote(setup.Service.Id));
        Assert.Equal(2, second.Version);
        Assert.Equal(20000m, second.Total);
        await Post<QuotationResponse>(http, path + $"/quotations/{quote.Id}/accept", null, HttpStatusCode.OK);
        var close = new CloseOpportunityRequest { AvailabilityConfirmed = true, Reason = "Aceptación y anticipo verificados" };
        Assert.Equal(HttpStatusCode.Conflict, (await http.PostAsJsonAsync(path + "/win", close)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await http.PostAsJsonAsync(path + "/win",
            new CloseOpportunityRequest { Reason = "Sin disponibilidad" })).StatusCode);

        // Changing the current company cannot bypass the policy captured in the accepted version.
        var municipality = await Post<CompanyResponse>(http, "/api/companies", new CompanyRequest
        {
            Name = "Municipalidad posterior", Type = CompanyType.Municipality
        });
        await Put<ClientResponse>(http, $"/api/clients/{setup.Client.Id}", new ClientDetailsRequest
        {
            FirstName = setup.Client.FirstName, Phone = setup.Client.Phone, CompanyId = municipality.Id
        });
        Assert.Equal(HttpStatusCode.Conflict, (await http.PostAsJsonAsync(path + "/win", close)).StatusCode);

        await Post<DepositResponse>(http, path + "/deposits", Deposit(4999.99m));
        Assert.Equal(HttpStatusCode.Conflict, (await http.PostAsJsonAsync(path + "/win", close)).StatusCode);
        await Post<DepositResponse>(http, path + "/deposits", Deposit(0.01m));
        var result = await Post<EventResponse>(http, path + "/win", close, HttpStatusCode.OK);
        Assert.Equal(EventStatus.Pending, result.Status);
        Assert.Equal(quote.Id, result.QuotationId);
        Assert.Equal(10000m, result.Total);
        var retry = await Post<EventResponse>(http, path + "/win", close, HttpStatusCode.OK);
        Assert.Equal(result.Id, retry.Id);

        var detail = await Get<OpportunityResponse>(http, path);
        Assert.Equal(CommercialStatus.Won, detail.Status);
        Assert.Equal(close.Reason, detail.ClosureReason);
        Assert.Equal(2, detail.Quotations.Count);
        Assert.Equal(quote.Text, detail.Quotations[0].Text);
        Assert.Equal(quote.Items[0].ServiceName, detail.Quotations[0].Items[0].ServiceName);
        Assert.Equal(10000m, detail.Quotations[0].Total);
        Assert.Equal(5000m, detail.Deposits.Sum(d => d.Amount));
        Assert.Equal(HttpStatusCode.Conflict, (await http.DeleteAsync(path)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await http.DeleteAsync($"/api/clients/{setup.Client.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await http.DeleteAsync($"/api/services/{setup.Service.Id}")).StatusCode);

        var digits = new string(setup.Client.Phone.Where(char.IsAsciiDigit).ToArray());
        var found = await Get<List<OpportunityResponse>>(http, $"/api/opportunities?search={digits}");
        Assert.Contains(found, o => o.Id == opportunity.Id);
        var events = await Get<List<EventResponse>>(http, $"/api/events?search={setup.Client.FirstName}");
        Assert.Single(events, e => e.Id == result.Id);
        var completed = await Put<EventResponse>(http, $"/api/events/{result.Id}", new EventUpdateRequest { Status = EventStatus.Completed });
        Assert.NotNull(completed.CompletedAt);
        Assert.Equal(HttpStatusCode.Conflict, (await http.PutAsJsonAsync($"/api/events/{result.Id}",
            new EventUpdateRequest { Status = EventStatus.Pending })).StatusCode);
    }

    [Fact]
    public async Task MunicipalityClosesWithoutDepositAndOnlyOneEventIsCreatedConcurrently()
    {
        using var http = Client();
        var company = await Post<CompanyResponse>(http, "/api/companies", new CompanyRequest
        {
            Name = "Municipalidad de prueba", Type = CompanyType.Municipality
        });
        var setup = await SetupAsync(http, company.Id);
        var opportunity = await CreateOpportunity(http, setup.Client.Id, setup.Service.Id);
        var path = $"/api/opportunities/{opportunity.Id}";
        var quote = await Post<QuotationResponse>(http, path + "/quotations", Quote(setup.Service.Id));
        Assert.True(quote.IsMunicipal);
        await Post<QuotationResponse>(http, path + $"/quotations/{quote.Id}/send", null, HttpStatusCode.OK);
        await Post<QuotationResponse>(http, path + $"/quotations/{quote.Id}/accept", null, HttpStatusCode.OK);
        Assert.Equal(HttpStatusCode.Conflict, (await http.PostAsJsonAsync(path + "/deposits", Deposit(5000))).StatusCode);
        var close = new CloseOpportunityRequest { AvailabilityConfirmed = true, Reason = "Contratación municipal aceptada" };
        var responses = await Task.WhenAll(Enumerable.Range(0, 4).Select(_ => http.PostAsJsonAsync(path + "/win", close)));
        Assert.Contains(responses, r => r.StatusCode == HttpStatusCode.OK);
        Assert.All(responses, r => Assert.Contains(r.StatusCode, new[] { HttpStatusCode.OK, HttpStatusCode.Conflict }));
        var events = await Get<List<EventResponse>>(http, $"/api/events?clientId={setup.Client.Id}");
        var result = Assert.Single(events);
        Assert.Equal(EventStatus.Pending, result.Status);
        var detail = await Get<OpportunityResponse>(http, path);
        Assert.Equal(CommercialStatus.Won, detail.Status);
        Assert.Empty(detail.Deposits);
        Assert.Equal(quote.Id, result.QuotationId);
        Assert.Equal(HttpStatusCode.BadRequest, (await http.PutAsJsonAsync($"/api/events/{result.Id}",
            new EventUpdateRequest { Status = EventStatus.Cancelled })).StatusCode);
        var cancelled = await Put<EventResponse>(http, $"/api/events/{result.Id}",
            new EventUpdateRequest { Status = EventStatus.Cancelled, CancellationReason = "Cambio de fecha" });
        Assert.Equal("Cambio de fecha", cancelled.CancellationReason);
        Assert.Equal(HttpStatusCode.MethodNotAllowed, (await http.DeleteAsync($"/api/events/{result.Id}")).StatusCode);
    }

    [Fact]
    public async Task CrudValidationPatchAndSearchRunThroughHttp()
    {
        using var http = Client();
        Assert.Equal(HttpStatusCode.BadRequest, (await http.PostAsJsonAsync("/api/clients", new { firstName = " ", phone = "bad" })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await http.PostAsJsonAsync("/api/companies", new { name = "Test", type = 999 })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await http.PostAsJsonAsync("/api/clients", new
        {
            firstName = "Test", phone = "88888888", companyId = int.MaxValue
        })).StatusCode);
        var setup = await SetupAsync(http);
        var url = $"/api/clients/{setup.Client.Id}";
        var patch = new StringContent("[{\"op\":\"replace\",\"path\":\"/lastName\",\"value\":\"Prueba\"}]", Encoding.UTF8, "application/json-patch+json");
        var response = await http.PatchAsync(url, patch);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Prueba", (await Read<ClientResponse>(response)).LastName);
        var invalidPatch = new StringContent("[{\"op\":\"remove\",\"path\":\"/firstName\"}]", Encoding.UTF8, "application/json-patch+json");
        Assert.Equal(HttpStatusCode.BadRequest, (await http.PatchAsync(url, invalidPatch)).StatusCode);
        Assert.Equal(setup.Client.FirstName, (await Get<ClientResponse>(http, url)).FirstName);
        var incomplete = await Post<OpportunityResponse>(http, "/api/opportunities", new OpportunityRequest { ClientId = setup.Client.Id });
        Assert.Null(incomplete.EventDate);
        Assert.Equal(HttpStatusCode.BadRequest, (await http.PostAsJsonAsync($"/api/opportunities/{incomplete.Id}/quotations", Quote(setup.Service.Id))).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await http.DeleteAsync($"/api/opportunities/{incomplete.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await http.DeleteAsync(url)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await http.GetAsync(url)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await http.DeleteAsync($"/api/pricing-profiles/{setup.Profile.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await http.DeleteAsync($"/api/services/{setup.Service.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await http.DeleteAsync($"/api/pricing-profiles/{setup.Profile.Id}")).StatusCode);
        var company = await Post<CompanyResponse>(http, "/api/companies", new CompanyRequest { Name = "Temporal" });
        await Put<CompanyResponse>(http, $"/api/companies/{company.Id}", new CompanyRequest { Name = "Editada", BaseRate = 5 });
        Assert.Equal(5m, (await Get<CompanyResponse>(http, $"/api/companies/{company.Id}")).BaseRate);
        Assert.Equal(HttpStatusCode.NoContent, (await http.DeleteAsync($"/api/companies/{company.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await http.GetAsync("/openapi/v1.json")).StatusCode);
    }

    [Fact]
    public async Task CalculatesSnapshotAndRecordsLossWithoutDeletingTheQuotation()
    {
        using var http = Client();
        var company = await Post<CompanyResponse>(http, "/api/companies", new CompanyRequest { Name = "Escuela", Type = CompanyType.School, BaseRate = 5 });
        var setup = await SetupAsync(http, company.Id);
        var opportunity = await CreateOpportunity(http, setup.Client.Id, setup.Service.Id);
        var path = $"/api/opportunities/{opportunity.Id}";
        var quote = await Post<QuotationResponse>(http, path + "/quotations", new QuotationRequest
        {
            Transport = 3000, Items = [new() { ServiceId = setup.Service.Id, Quantity = 2, DurationMinutes = 120, DiscountPercent = 15 }]
        });
        Assert.Equal(40000m, quote.Subtotal);
        Assert.Equal(6000m, quote.Discount);
        Assert.Equal(1700m, quote.Surcharge);
        Assert.Equal(38700m, quote.Total);
        var lost = await Post<OpportunityResponse>(http, path + "/lose", new ReasonRequest { Reason = "Cliente canceló la consulta" }, HttpStatusCode.OK);
        Assert.Equal(CommercialStatus.Lost, lost.Status);
        Assert.Single(lost.Quotations);
        Assert.NotNull(lost.ClosureReason);
        Assert.Equal(HttpStatusCode.Conflict, (await http.PostAsync(path + $"/quotations/{quote.Id}/send", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await http.DeleteAsync(path)).StatusCode);
        Assert.Equal(HttpStatusCode.MethodNotAllowed, (await http.DeleteAsync(path + $"/quotations/{quote.Id}")).StatusCode);
    }

    private static QuotationRequest Quote(int serviceId) => new() { Items = [new() { ServiceId = serviceId, DurationMinutes = 60 }] };
    private static DepositRequest Deposit(decimal amount) => new() { Amount = amount, ReceivedAt = DateTime.UtcNow.AddMinutes(-1), Reference = Guid.NewGuid().ToString() };
    private static Task<OpportunityResponse> CreateOpportunity(HttpClient http, int clientId, int serviceId) =>
        Post<OpportunityResponse>(http, "/api/opportunities", new OpportunityRequest
        {
            ClientId = clientId, EventDate = DateTime.UtcNow.AddDays(20), ServiceIds = [serviceId],
            Location = new() { Address = "Salón de eventos", Province = "Heredia" }
        });
    private static async Task<(ClientResponse Client, PricingProfileResponse Profile, ServiceResponse Service)> SetupAsync(HttpClient http, int? companyId = null)
    {
        var client = await Post<ClientResponse>(http, "/api/clients", new ClientDetailsRequest
        {
            FirstName = "Cliente" + Guid.NewGuid().ToString("N"), Phone = "+506 8888-1234", CompanyId = companyId
        });
        var profile = await Post<PricingProfileResponse>(http, "/api/pricing-profiles", new PricingProfileRequest { BasePrice = 10000, BaseDurationMinutes = 60 });
        var service = await Post<ServiceResponse>(http, "/api/services", new ServiceRequest { Name = "Animación", ServiceType = ServiceType.Entertainer, PricingProfileId = profile.Id });
        return (client, profile, service);
    }
    private static async Task<T> Post<T>(HttpClient http, string path, object? body, HttpStatusCode expected = HttpStatusCode.Created)
    {
        var response = body is null ? await http.PostAsync(path, null) : await http.PostAsJsonAsync(path, body, Json);
        Assert.True(response.StatusCode == expected, $"POST {path}: {(int)response.StatusCode} {await response.Content.ReadAsStringAsync()}");
        if (expected == HttpStatusCode.Created) Assert.NotNull(response.Headers.Location);
        return await Read<T>(response);
    }
    private static async Task<T> Put<T>(HttpClient http, string path, object body)
    {
        var response = await http.PutAsJsonAsync(path, body, Json);
        Assert.True(response.IsSuccessStatusCode, $"PUT {path}: {await response.Content.ReadAsStringAsync()}");
        return await Read<T>(response);
    }
    private static async Task<T> Get<T>(HttpClient http, string path)
    {
        var response = await http.GetAsync(path);
        Assert.True(response.IsSuccessStatusCode, $"GET {path}: {await response.Content.ReadAsStringAsync()}");
        return await Read<T>(response);
    }
    private static async Task<T> Read<T>(HttpResponseMessage response) => (await response.Content.ReadFromJsonAsync<T>(Json))!;
}
