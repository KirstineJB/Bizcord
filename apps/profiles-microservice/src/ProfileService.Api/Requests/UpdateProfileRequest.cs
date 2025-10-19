using System.ComponentModel.DataAnnotations;

namespace ProfileService.Api.Requests;

public class UpdateProfileRequest
{
    [Required, MinLength(2)]
    public string DisplayName { get; set; } = default!;

    [Required, EmailAddress]
    public string Email { get; set; } = default!;

    public string? Bio { get; set; }
}