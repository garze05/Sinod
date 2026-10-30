using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using Microsoft.AspNetCore.Mvc;
using Sinod.Api.Contracts.Clients;
using Sinod.Api.Services;

namespace Sinod.Api.Endpoints;

public static class ClientEndpoints
{
    public static void MapClientEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/clients").WithTags("Clients");
        group.MapGet("/", async (string? search, ClientService service, CancellationToken ct) =>
            TypedResults.Ok(await service.SearchAsync(search, ct)));
        group.MapGet("/{id:int}", async (int id, ClientService service, CancellationToken ct) =>
            TypedResults.Ok(await service.GetByIdAsync(id, ct) ?? throw ApiException.NotFound("Client")));
        group.MapPost("/", async (ClientDetailsRequest request, ClientService service, CancellationToken ct) =>
            Write(await service.CreateAsync(request, ct), created: true)).Produces<ClientResponse>(201);
        group.MapPut("/{id:int}", async (int id, ClientDetailsRequest request, ClientService service, CancellationToken ct) =>
            Write(await service.UpdateAsync(id, request, ct))).Produces<ClientResponse>();
        group.MapPatch("/{id:int}", PatchAsync).Accepts<JsonPatchDocument<ClientDetailsRequest>>("application/json-patch+json")
            .Produces<ClientResponse>().ProducesValidationProblem().DisableValidation();
        group.MapDelete("/{id:int}", async (int id, ClientService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return TypedResults.NoContent();
        });
    }

    private static async Task<IResult> PatchAsync(int id, [FromBody] JsonPatchDocument<ClientDetailsRequest> patch,
        ClientService service, CancellationToken ct)
    {
        string[] paths = ["/firstName", "/lastName", "/email", "/phone", "/companyId"];
        if (patch.Operations.Count is 0 or > 5 || patch.Operations.Any(o =>
                o.op is not ("replace" or "remove") || !paths.Contains(o.path, StringComparer.OrdinalIgnoreCase)))
            throw ApiException.Invalid("Send 1 to 5 replace/remove operations on editable client fields.");
        var current = await service.GetByIdAsync(id, ct) ?? throw ApiException.NotFound("Client");
        var request = new ClientDetailsRequest { FirstName = current.FirstName, LastName = current.LastName,
            Email = current.Email, Phone = current.Phone, CompanyId = current.CompanyId };
        var errors = new List<string>();
        patch.ApplyTo(request, error => errors.Add(error.ErrorMessage));
        if (errors.Count > 0) return Results.ValidationProblem(new Dictionary<string, string[]> { ["patch"] = errors.ToArray() });
        var validation = new List<ValidationResult>();
        if (!Validator.TryValidateObject(request, new ValidationContext(request), validation, true))
            return Results.ValidationProblem(validation.SelectMany(v => v.MemberNames.DefaultIfEmpty("patch")
                    .Select(name => (name, error: v.ErrorMessage ?? "Invalid value.")))
                .GroupBy(v => v.name).ToDictionary(g => g.Key, g => g.Select(v => v.error).ToArray()));
        return Write(await service.UpdateAsync(id, request, ct));
    }

    private static IResult Write(ClientWriteResult result, bool created = false)
    {
        if (result.Error == ClientWriteError.ClientNotFound) throw ApiException.NotFound("Client");
        if (result.Error == ClientWriteError.CompanyNotFound) throw ApiException.Invalid("The company does not exist.");
        var client = result.Client ?? throw new InvalidOperationException("Client write returned no client.");
        return created ? Results.Created($"/api/clients/{client.Id}", client) : Results.Ok(client);
    }
}
