

namespace ProfileService.Application.Dtos;

public record UserProfileDto(Guid Id, string Username, string DisplayName, string Email, string? Bio);