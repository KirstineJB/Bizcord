using Microsoft.AspNetCore.Mvc;
using Polly;
using Polly.Fallback;
using Polly.Retry;
using ProfileService.Api.Messaging;
using ProfileService.Api.Requests;
using ProfileService.Application.Contracts;
using ProfileService.Application.Dtos;
using ProfileService.Contracts;
using ProfileService.Domain.Events;
using System.ComponentModel.DataAnnotations;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace ProfileService.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ProfilesController : ControllerBase
{
    private readonly IUserProfileService _service;
    private readonly IMessageClient _bus;


    // Prøver 3 gange
    private static readonly AsyncRetryPolicy<IReadOnlyList<UserProfileDto>> RetryPolicy =
        Policy<IReadOnlyList<UserProfileDto>>
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(200 * attempt)
            );

    // Fallback: Hvis retry fejler 3 gange returneres end tom liste
    private static readonly AsyncFallbackPolicy<IReadOnlyList<UserProfileDto>> FallbackPolicy =
        Policy<IReadOnlyList<UserProfileDto>>
            .Handle<Exception>()
            .FallbackAsync(
                fallbackAction: ct =>
                {
                    return Task.FromResult<IReadOnlyList<UserProfileDto>>(Array.Empty<UserProfileDto>());
                }
            );

    // kombineret resiliency policy
    private static readonly IAsyncPolicy<IReadOnlyList<UserProfileDto>> ResiliencyPolicy =
        Policy.WrapAsync(FallbackPolicy, RetryPolicy);

    public ProfilesController(IUserProfileService service, IMessageClient bus)
    {
        _service = service;
        _bus = bus;
    }


    [HttpPost]
    public async Task<ActionResult<ProfileSharedDto>> Create([FromBody] CreateProfileRequest req, CancellationToken ct)
    {
        try
        {
            var created = await _service.CreateAsync(req.Username, req.DisplayName, req.Email, req.Bio, ct);


            await _bus.PublishAsync(
                new Contracts.ProfileCreated(created.Id, created.Username, created.DisplayName, created.Email, DateTimeOffset.UtcNow),
                topic: "profiles.created",
                ct: ct);

            var shared = new ProfileSharedDto(created.Id, created.Username, created.DisplayName, created.Email, DateTimeOffset.UtcNow);
            return CreatedAtAction(nameof(GetById), new { id = shared.Id }, shared);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }


    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProfileSharedDto>> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var dto = await _service.GetAsync(id, ct);
        if (dto is null) return NotFound();

        var shared = new ProfileSharedDto(dto.Id, dto.Username, dto.DisplayName, dto.Email, DateTimeOffset.UtcNow);
        return Ok(shared);
    }

    //Simulates et endpoint kun internal services may call
    [HttpGet("internal/all")]
    public async Task<ActionResult<IEnumerable<ProfileSharedDto>>> GetAllInternal(
    [FromHeader(Name = "X-UserRole")] string? role,
    [FromHeader(Name = "X-ServiceId")] string? serviceId,
    CancellationToken ct = default)
    {
        // Check the role is service or else no go
        if (role != "Service")
        {

            return Forbid();
        }

        Console.WriteLine($"Internal call from service: {serviceId ?? "<unknown>"}");

        var list = await _service.ListAsync(0, 100, ct);
        var shared = list.Select(p =>
            new ProfileSharedDto(p.Id, p.Username, p.DisplayName, p.Email, DateTimeOffset.UtcNow));

        return Ok(shared);
    }


    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProfileSharedDto>>> List([FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        var userId = HttpContext.Request.Headers["X-UserId"].FirstOrDefault();
        var role = HttpContext.Request.Headers["X-UserRole"].FirstOrDefault();
    
        if (role != "Admin")
        {
            return Forbid();
        }

        var list = await ResiliencyPolicy.ExecuteAsync(
                () => _service.ListAsync(skip, Math.Clamp(take, 1, 100), ct)
            );
        var shared = list.Select(p => new ProfileSharedDto(p.Id, p.Username, p.DisplayName, p.Email, DateTimeOffset.UtcNow));
        return Ok(shared);
    }


    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateProfileRequest req, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        try
        {
            var (found, updated, changed) =
                await _service.UpdateAsync(id, req.DisplayName, req.Email, req.Bio,  ct);

            if (!found) return NotFound();

            // Publish integration event only when something actually changed
            if (changed.Count > 0 && updated is not null)
            {
                await _bus.PublishAsync(
                    new Contracts.ProfileUpdated(
                        updated.Id,
                        updated.DisplayName,
                        updated.Email,
                        DateTimeOffset.UtcNow,
                        changed.ToArray()),
                    topic: "profiles.updated",
                    ct: ct);
            }

          
            var shared = new ProfileSharedDto(
                updated!.Id,
                updated.Username,
                updated.DisplayName,
                updated.Email,
                updated.CreatedAt);

            return Ok(shared);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        var ok = await _service.DeleteAsync(id, ct);
        return ok ? NoContent() : NotFound();
    }
}