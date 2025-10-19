using ProfileService.Domain.Entities;

namespace ProfileService.Domain.Repositories;

public interface IUserProfileRepository
{
    Task<UserProfile?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<UserProfile?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<IReadOnlyList<UserProfile>> ListAsync(int skip = 0, int take = 50, CancellationToken ct = default);
    Task AddAsync(UserProfile profile, CancellationToken ct = default);
    Task UpdateAsync(UserProfile profile, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}