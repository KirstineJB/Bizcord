using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Linq;
using System.Threading;
using ProfileService.Domain.Entities;
using ProfileService.Domain.Events;
using ProfileService.Domain.ValueObjects;
using Xunit;

namespace ProfileService.UnitTests.Domain;

public class UserProfileTests
{
    [Fact]
    public void Update_TracksChangedFields_AndRaisesEvent_WhenSomethingChanges()
    {
        // Ar
        var profile = UserProfile.Create(
            username: "kirst",
            displayName: "Kirst Old",
            email: Email.Create("old@example.com"),
            bio: "Old bio"
        );

        var before = profile.UpdatedAt;

        // Act
        var changed = profile.Update(
            displayName: "Kirst New",
            email: Email.Create("new@example.com"),
            bio: "New bio"
        );

        // As
        Assert.Contains(nameof(UserProfile.DisplayName), changed);
        Assert.Contains(nameof(UserProfile.Email), changed);
        Assert.Contains(nameof(UserProfile.Bio), changed);
        Assert.True(profile.UpdatedAt > before);

        // Domain event raised
        var updateEvents = profile.DomainEvents.OfType<ProfileUpdated>().ToList();
        Assert.Single(updateEvents);
    }

    [Fact]
    public void Update_NoChanges_ReturnsEmpty_AndDoesNotRaiseEvent()
    {
        // Arr
        var profile = UserProfile.Create(
            username: "kirst",
            displayName: "Kirst",
            email: Email.Create("kirst@example.com"),
            bio: "Same"
        );
        profile.ClearDomainEvents(); 

        var beforeUpdatedAt = profile.UpdatedAt;

        // Act
        var changed = profile.Update(
            displayName: "Kirst",
            email: Email.Create("kirst@example.com"),
            bio: "Same"
        );

        // As
        Assert.Empty(changed);
        Assert.Empty(profile.DomainEvents);
        Assert.True(profile.UpdatedAt >= beforeUpdatedAt); 
    }

    [Fact]
    public void Create_InvalidEmail_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            UserProfile.Create("user", "User", Email.Create("not-an-email")));
    }
}