using Microsoft.Extensions.Diagnostics.HealthChecks;
using Sinod.Api.Contracts;

namespace Sinod.Api.Endpoints;

public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/health", async (HealthCheckService service, HttpContext context, CancellationToken ct) =>
        {
            context.Response.Headers.CacheControl = "no-store, no-cache";
            var report = await service.CheckHealthAsync(r => r.Name == "postgresql", ct);
            return Results.Json(new HealthResponse(report.Status.ToString()), statusCode:
                report.Status == HealthStatus.Healthy ? 200 : 503);
        }).WithTags("Health").Produces<HealthResponse>().Produces<HealthResponse>(503);
    }
}
