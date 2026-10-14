using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Sinod.Api.Contracts;

namespace Sinod.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController(HealthCheckService healthCheckService) : ControllerBase
    {
        [HttpGet]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [ProducesResponseType<HealthResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<HealthResponse>(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> GetHealthStatus(CancellationToken cancellationToken)
        {
            var report = await healthCheckService.CheckHealthAsync(
                registration => registration.Name == "postgresql", cancellationToken);

            var statusCode = report.Status == HealthStatus.Healthy
                ? StatusCodes.Status200OK
                : StatusCodes.Status503ServiceUnavailable;

            return StatusCode(statusCode, new HealthResponse(report.Status.ToString()));
        }
    }
}
