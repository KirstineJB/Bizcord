using ProfileService.Domain.Entities;
using ProfileService.Domain.Repositories;

namespace ProfileService.Infrastructure.Persistence;

public sealed class InMemoryUserProfileRepository : IUserProfileRepository
{
    private readonly Dictionary<Guid, UserProfile> _byId = new();
    private readonly Dictionary<string, Guid> _byUsername = new(StringComparer.OrdinalIgnoreCase);

    public Task<UserProfile?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(_byId.TryGetValue(id, out var p) ? p : null);

    public Task<UserProfile?> GetByUsernameAsync(string username, CancellationToken ct = default) =>
        Task.FromResult(_byUsername.TryGetValue(username, out var id) && _byId.TryGetValue(id, out var p) ? p : null);

    public Task<IReadOnlyList<UserProfile>> ListAsync(int skip = 0, int take = 50, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<UserProfile>>(_byId.Values.Skip(skip).Take(take).ToList());

    public Task AddAsync(UserProfile profile, CancellationToken ct = default)
    {
        _byId[profile.Id] = profile;
        _byUsername[profile.Username] = profile.Id;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(UserProfile profile, CancellationToken ct = default)
    {
        _byId[profile.Id] = profile;
        _byUsername[profile.Username] = profile.Id;
        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        if (!_byId.TryGetValue(id, out var profile))
            return Task.FromResult(false);

        _byId.Remove(id);
        _byUsername.Remove(profile.Username);
        return Task.FromResult(true);
    }
}