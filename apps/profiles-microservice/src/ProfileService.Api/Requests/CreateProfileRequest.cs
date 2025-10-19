using System.ComponentModel.DataAnnotations;

namespace ProfileService.Api.Requests;

public class CreateProfileRequest
{
    [Required, MinLength(3)]
    public string Username { get; set; } = default!;

    [Required, MinLength(2)]
    public string DisplayName { get; set; } = default!;

    [Required, EmailAddress]
    public string Email { get; set; } = default!;

    public string? Bio { get; set; }
}