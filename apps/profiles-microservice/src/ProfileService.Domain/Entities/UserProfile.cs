using ProfileService.Domain.Common;
using ProfileService.Domain.Events;
using ProfileService.Domain.ValueObjects;

namespace ProfileService.Domain.Entities;

public sealed class UserProfile : Entity<Guid>
{
    public string Username { get; private set; } = default!;
    public string DisplayName { get; private set; } = default!;
    public Email Email { get; private set; } = default!;
    public string? Bio { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private UserProfile() { } // required for serializers/ORM

    private UserProfile(Guid id, string username, string displayName, Email email, string? bio)
    {
        if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Username required.");
        if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentException("Display name required.");

        Id = id;
        Username = username.Trim();
        DisplayName = displayName.Trim();
        Email = email;
        Bio = string.IsNullOrWhiteSpace(bio) ? null : bio.Trim();
        CreatedAt = UpdatedAt = DateTimeOffset.UtcNow;

        Raise(new ProfileCreated(Id, Username, DisplayName, Email.Value));
    }

    public static UserProfile Create(string username, string displayName, Email email, string? bio = null)
        => new(Guid.NewGuid(), username, displayName, email, bio);

    public void Update(string displayName, Email email, string? bio)
    {
        if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentException("Display name required.");

        DisplayName = displayName.Trim();
        Email = email;
        Bio = string.IsNullOrWhiteSpace(bio) ? null : bio.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;

        Raise(new ProfileUpdated(Id, DisplayName, Email.Value));
    }
}