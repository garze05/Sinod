using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Sinod.Api.Contracts;

namespace Sinod.Tests;

public class HealthEndpointTests
{
    [Theory]
    [InlineData(true, HttpStatusCode.OK, "Healthy")]
    [InlineData(false, HttpStatusCode.ServiceUnavailable, "Unhealthy")]
    public async Task HealthDependsOnlyOnDatabaseConnectivity(bool available, HttpStatusCode status, string expected)
    {
        await using var factory = new HealthFactory(available);
        using var client = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost") });
        var response = await client.GetAsync("/api/health");
        Assert.Equal(status, response.StatusCode);
        Assert.Equal(expected, (await response.Content.ReadFromJsonAsync<HealthResponse>())!.Status);
        Assert.True(response.Headers.CacheControl!.NoStore);
    }

    private sealed class HealthFactory(bool available) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("ConnectionStrings:Sinod", "Host=localhost;Database=unused");
            builder.ConfigureServices(services => services.Configure<HealthCheckServiceOptions>(options =>
            {
                options.Registrations.Clear();
                options.Registrations.Add(new("postgresql", _ => new StubCheck(available), null, null));
                options.Registrations.Add(new("unrelated-service", _ => new StubCheck(false), null, null));
            }));
        }
    }

    private sealed class StubCheck(bool available) : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default) =>
            Task.FromResult(available ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy());
    }
}
