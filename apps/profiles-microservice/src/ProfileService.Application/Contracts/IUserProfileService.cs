using ProfileService.Application.Dtos;
using ProfileService.Domain.Entities;
using ProfileService.Domain.Repositories;
using ProfileService.Domain.ValueObjects;

namespace ProfileService.Application.Contracts;

public interface IUserProfileService
{
    Task<UserProfileDto> CreateAsync(string username, string displayName, string email, string? bio, CancellationToken ct);
    Task<UserProfileDto?> GetAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<UserProfileDto>> ListAsync(int skip, int take, CancellationToken ct);
    Task<bool> UpdateAsync(Guid id, string displayName, string email, string? bio, CancellationToken ct);

    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}

public class UserProfileService : IUserProfileService
{
    private readonly IUserProfileRepository _repo;
    public UserProfileService(IUserProfileRepository repo) => _repo = repo;

    public async Task<UserProfileDto> CreateAsync(string username, string displayName, string email, string? bio, CancellationToken ct)
    {
        if (await _repo.GetByUsernameAsync(username, ct) is not null)
            throw new InvalidOperationException("Username already exists.");

        var p = UserProfile.Create(username, displayName, Email.Create(email), bio);
        await _repo.AddAsync(p, ct);
        return new UserProfileDto(p.Id, p.Username, p.DisplayName, p.Email.Value, p.Bio);
    }
    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        return await _repo.DeleteAsync(id, ct);
    }
    public async Task<UserProfileDto?> GetAsync(Guid id, CancellationToken ct)
    {
        var p = await _repo.GetByIdAsync(id, ct);
        return p is null ? null : new UserProfileDto(p.Id, p.Username, p.DisplayName, p.Email.Value, p.Bio);
    }

    public async Task<IReadOnlyList<UserProfileDto>> ListAsync(int skip, int take, CancellationToken ct)
    {
        var list = await _repo.ListAsync(skip, take, ct);
        return list.Select(p => new UserProfileDto(p.Id, p.Username, p.DisplayName, p.Email.Value, p.Bio)).ToList();
    }

    public async Task<bool> UpdateAsync(Guid id, string displayName, string email, string? bio, CancellationToken ct)
    {
        var p = await _repo.GetByIdAsync(id, ct);
        if (p is null) return false;
        p.Update(displayName, Email.Create(email), bio);
        await _repo.UpdateAsync(p, ct);
        return true;
    }
}