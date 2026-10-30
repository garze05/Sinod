using Sinod.Api.Contracts;
using Sinod.Api.Services;

namespace Sinod.Api.Endpoints;

public static class CatalogEndpoints
{
    public static void MapCatalogEndpoints(this IEndpointRouteBuilder app)
    {

        var companies = app.MapGroup("/api/companies").WithTags("Companies");
        companies.MapGet("/", async (CatalogService service, CancellationToken ct) => TypedResults.Ok(await service.CompaniesAsync(ct)));
        companies.MapGet("/{id:int}", async (int id, CatalogService service, CancellationToken ct) => TypedResults.Ok(await service.CompanyAsync(id, ct)));
        companies.MapPost("/", async (CompanyRequest request, CatalogService service, CancellationToken ct) =>
        {
            var item = await service.SaveCompanyAsync(null, request, ct);
            return TypedResults.Created($"/api/companies/{item.Id}", item);
        });
        companies.MapPut("/{id:int}", async (int id, CompanyRequest request, CatalogService service, CancellationToken ct) =>
            TypedResults.Ok(await service.SaveCompanyAsync(id, request, ct)));
        companies.MapDelete("/{id:int}", async (int id, CatalogService service, CancellationToken ct) =>
        {
            await service.DeleteCompanyAsync(id, ct);
            return TypedResults.NoContent();
        });

        var pricingprofiles = app.MapGroup("/api/pricing-profiles").WithTags("Pricing profiles");
        pricingprofiles.MapGet("/", async (CatalogService service, CancellationToken ct) => TypedResults.Ok(await service.ProfilesAsync(ct)));
        pricingprofiles.MapGet("/{id:int}", async (int id, CatalogService service, CancellationToken ct) => TypedResults.Ok(await service.ProfileAsync(id, ct)));
        pricingprofiles.MapPost("/", async (PricingProfileRequest request, CatalogService service, CancellationToken ct) =>
        {
            var item = await service.SaveProfileAsync(null, request, ct);
            return TypedResults.Created($"/api/pricing-profiles/{item.Id}", item);
        });
        pricingprofiles.MapPut("/{id:int}", async (int id, PricingProfileRequest request, CatalogService service, CancellationToken ct) =>
            TypedResults.Ok(await service.SaveProfileAsync(id, request, ct)));
        pricingprofiles.MapDelete("/{id:int}", async (int id, CatalogService service, CancellationToken ct) =>
        {
            await service.DeleteProfileAsync(id, ct);
            return TypedResults.NoContent();
        });

        var services = app.MapGroup("/api/services").WithTags("Services");
        services.MapGet("/", async (CatalogService service, CancellationToken ct) => TypedResults.Ok(await service.ServicesAsync(ct)));
        services.MapGet("/{id:int}", async (int id, CatalogService service, CancellationToken ct) => TypedResults.Ok(await service.ServiceAsync(id, ct)));
        services.MapPost("/", async (ServiceRequest request, CatalogService service, CancellationToken ct) =>
        {
            var item = await service.SaveServiceAsync(null, request, ct);
            return TypedResults.Created($"/api/services/{item.Id}", item);
        });
        services.MapPut("/{id:int}", async (int id, ServiceRequest request, CatalogService service, CancellationToken ct) =>
            TypedResults.Ok(await service.SaveServiceAsync(id, request, ct)));
        services.MapDelete("/{id:int}", async (int id, CatalogService service, CancellationToken ct) =>
        {
            await service.DeleteServiceAsync(id, ct);
            return TypedResults.NoContent();
        });
    }
}
