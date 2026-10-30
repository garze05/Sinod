using Sinod.Api.Contracts;
using Sinod.Api.Services;

namespace Sinod.Api.Endpoints;

public static class CommercialEndpoints
{
    public static void MapCommercialEndpoints(this IEndpointRouteBuilder app)
    {
        var opportunities = app.MapGroup("/api/opportunities").WithTags("Opportunities");
        opportunities.MapGet("/", async (string? search, int? clientId, CommercialService service, CancellationToken ct) =>
            TypedResults.Ok(await service.SearchAsync(search, clientId, ct)));
        opportunities.MapGet("/{id:int}", async (int id, CommercialService service, CancellationToken ct) =>
            TypedResults.Ok(await service.GetAsync(id, ct)));
        opportunities.MapPost("/", async (OpportunityRequest request, CommercialService service, CancellationToken ct) =>
        {
            var opportunity = await service.SaveAsync(null, request, ct);
            return TypedResults.Created($"/api/opportunities/{opportunity.Id}", opportunity);
        });
        opportunities.MapPut("/{id:int}", async (int id, OpportunityRequest request, CommercialService service, CancellationToken ct) =>
            TypedResults.Ok(await service.SaveAsync(id, request, ct)));
        opportunities.MapDelete("/{id:int}", async (int id, CommercialService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return TypedResults.NoContent();
        });
        opportunities.MapPost("/{id:int}/quotations", async (int id, QuotationRequest request, CommercialService service, CancellationToken ct) =>
        {
            var quotation = await service.QuoteAsync(id, request, ct);
            return TypedResults.Created($"/api/opportunities/{id}/quotations/{quotation.Id}", quotation);
        });
        opportunities.MapGet("/{id:int}/quotations", async (int id, CommercialService service, CancellationToken ct) =>
            TypedResults.Ok((await service.GetAsync(id, ct)).Quotations));
        opportunities.MapGet("/{id:int}/quotations/{quotationId:int}", async (int id, int quotationId, CommercialService service, CancellationToken ct) =>
            TypedResults.Ok(await service.QuotationAsync(id, quotationId, ct)));
        opportunities.MapPost("/{id:int}/quotations/{quotationId:int}/send", async (int id, int quotationId, CommercialService service, CancellationToken ct) =>
            TypedResults.Ok(await service.SendAsync(id, quotationId, ct)));
        opportunities.MapPost("/{id:int}/quotations/{quotationId:int}/accept", async (int id, int quotationId, CommercialService service, CancellationToken ct) =>
            TypedResults.Ok(await service.AcceptAsync(id, quotationId, ct)));
        opportunities.MapGet("/{id:int}/deposits", async (int id, CommercialService service, CancellationToken ct) =>
            TypedResults.Ok((await service.GetAsync(id, ct)).Deposits));
        opportunities.MapPost("/{id:int}/deposits", async (int id, DepositRequest request, CommercialService service, CancellationToken ct) =>
        {
            var deposit = await service.DepositAsync(id, request, ct);
            return TypedResults.Created($"/api/opportunities/{id}/deposits", deposit);
        });
        opportunities.MapPost("/{id:int}/win", async (int id, CloseOpportunityRequest request, CommercialService service, CancellationToken ct) =>
            TypedResults.Ok(await service.WinAsync(id, request, ct)));
        opportunities.MapPost("/{id:int}/lose", async (int id, ReasonRequest request, CommercialService service, CancellationToken ct) =>
            TypedResults.Ok(await service.LoseAsync(id, request, ct)));

        var events = app.MapGroup("/api/events").WithTags("Events");
        events.MapGet("/", async (string? search, int? clientId, CommercialService service, CancellationToken ct) =>
            TypedResults.Ok(await service.EventsAsync(search, clientId, ct)));
        events.MapGet("/{id:int}", async (int id, CommercialService service, CancellationToken ct) =>
            TypedResults.Ok(await service.EventAsync(id, ct)));
        events.MapPut("/{id:int}", async (int id, EventUpdateRequest request, CommercialService service, CancellationToken ct) =>
            TypedResults.Ok(await service.UpdateEventAsync(id, request, ct)));
    }
}
