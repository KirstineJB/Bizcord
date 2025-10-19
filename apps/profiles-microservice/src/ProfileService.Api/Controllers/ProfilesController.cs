using Microsoft.AspNetCore.Mvc;
using ProfileService.Api.Messaging;
using ProfileService.Api.Requests;
using ProfileService.Application.Contracts;
using ProfileService.Contracts;


namespace ProfileService.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ProfilesController : ControllerBase
{
    private readonly IUserProfileService _service;
    private readonly IMessageClient _bus;

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
                new ProfileCreated(created.Id, created.Username, created.DisplayName, created.Email, DateTimeOffset.UtcNow),
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


    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProfileSharedDto>>> List([FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        var list = await _service.ListAsync(skip, Math.Clamp(take, 1, 100), ct);
        var shared = list.Select(p => new ProfileSharedDto(p.Id, p.Username, p.DisplayName, p.Email, DateTimeOffset.UtcNow));
        return Ok(shared);
    }

 
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateProfileRequest req, CancellationToken ct)
    {
        try
        {
            var ok = await _service.UpdateAsync(id, req.DisplayName, req.Email, req.Bio, ct);
            if (!ok) return NotFound();

      
            await _bus.PublishAsync(
                new ProfileUpdated(id, req.DisplayName, req.Email, DateTimeOffset.UtcNow),
                topic: "profiles.updated",
                ct: ct);

            return NoContent();
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