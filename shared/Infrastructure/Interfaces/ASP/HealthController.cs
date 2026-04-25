using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Frock_backend.shared.Infrastructure.Interfaces.ASP;

[ApiController]
[Route("[controller]")]
[Tags("Health")]
public class HealthController(HealthCheckService healthCheckService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetHealth()
    {
        var report = await healthCheckService.CheckHealthAsync();
        var result = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds
            }),
            totalDuration = report.TotalDuration.TotalMilliseconds
        };

        return report.Status == HealthStatus.Healthy ? Ok(result) : StatusCode(503, result);
    }

    [HttpGet("ready")]
    public async Task<IActionResult> GetReady()
    {
        var report = await healthCheckService.CheckHealthAsync(
            predicate: check => check.Tags.Contains("ready"));
        return report.Status == HealthStatus.Healthy
            ? Ok(new { status = "ready" })
            : StatusCode(503, new { status = "not ready" });
    }
}
