using ProfileService.Application.Dtos;
using ProfileService.Contracts;
using ProfileService.Domain.Entities;

namespace ProfileService.Application.Mappers;

public static class ProfileMappings
{
    public static ProfileSharedDto ToShared(this UserProfile p) =>
        new(p.Id, p.Username, p.DisplayName, p.Email.Value, p.CreatedAt);


}