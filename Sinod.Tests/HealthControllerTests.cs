using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Sinod.Api.Contracts;
using Sinod.Api.Controllers;

namespace Sinod.Tests;

public class HealthControllerTests
{
    [Theory]
    [InlineData(true, 200, "Healthy")]
    [InlineData(false, 503, "Unhealthy")]
    public async Task HealthDependsOnlyOnDatabaseConnectivity(
        bool databaseAvailable, int expectedStatusCode, string expectedStatus)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHealthChecks()
            .AddCheck("postgresql", () => databaseAvailable
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy())
            .AddCheck("unrelated-service", () => HealthCheckResult.Unhealthy());

        using var provider = services.BuildServiceProvider();
        var controller = new HealthController(provider.GetRequiredService<HealthCheckService>());

        var action = await controller.GetHealthStatus(CancellationToken.None);

        var result = Assert.IsType<ObjectResult>(action);
        Assert.Equal(expectedStatusCode, result.StatusCode);
        var body = Assert.IsType<HealthResponse>(result.Value);
        Assert.Equal(expectedStatus, body.Status);
    }
}
