using Microsoft.AspNetCore.Mvc;
using ProfileService.Api.Messaging;

namespace ProfileService.Api.Controllers;

[ApiController]
[Route("api/diagnostics")]
public class DiagnosticsController : ControllerBase
{
    private readonly IMessageClient _client;

    public DiagnosticsController(IMessageClient client)
    {
        _client = client;
    }

    // POST /api/diagnostics/publish-test?topic=profiles.test
    [HttpPost("publish-test")]
    public async Task<IActionResult> PublishTest([FromQuery] string topic = "profiles.test")
    {
        var payload = new
        {
            Msg = "Hello from profiles-microservice",
            When = DateTimeOffset.UtcNow
        };

        await _client.PublishAsync(payload, topic, HttpContext.RequestAborted);

        return Ok(new { published = true, topic });
    }
}