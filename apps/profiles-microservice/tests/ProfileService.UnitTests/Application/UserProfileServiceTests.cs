using Moq;
using ProfileService.Application.Contracts;
using ProfileService.Domain.Entities;
using ProfileService.Domain.Repositories;
using ProfileService.Domain.ValueObjects;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ProfileService.UnitTests.Application;

public class UserProfileServiceTests
{
    [Fact]
    public async Task CreateAsync_Throws_When_Username_Already_Exists()
    {

        var repo = new Mock<IUserProfileRepository>();
        var existing = UserProfile.Create("kirst", "Kirst", Email.Create("kirst@test.com"));
        repo.Setup(r => r.GetByUsernameAsync("kirst", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var svc = new UserProfileService(repo.Object);


        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.CreateAsync("kirst", "Another", "another@test.com", null, CancellationToken.None));

        repo.Verify(r => r.AddAsync(It.IsAny<UserProfile>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ReturnsFalse()
    {
        // Arr
        var repo = new Mock<IUserProfileRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserProfile?)null);

        var svc = new UserProfileService(repo.Object);

        // Act
        var (found, updated, changed) = await svc.UpdateAsync(
            Guid.NewGuid(), "Name", "n@test.com", null,  CancellationToken.None);

        // As
        Assert.False(found);
        Assert.Null(updated);
        Assert.Empty(changed);
        repo.Verify(r => r.UpdateAsync(It.IsAny<UserProfile>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ChangesArePersisted_AndReturned()
    {
        // Arr
        var profile = UserProfile.Create("kirst", "Old", Email.Create("old@test.com"), "bio") ;
        var repo = new Mock<IUserProfileRepository>();
        repo.Setup(r => r.GetByIdAsync(profile.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        var svc = new UserProfileService(repo.Object);

        // Act
        var (found, updated, changed) = await svc.UpdateAsync(
            profile.Id, "New", "new@test.com", "new bio", CancellationToken.None);

        // As
        Assert.True(found);
        Assert.NotNull(updated);
        Assert.Contains("DisplayName", changed);
        Assert.Contains("Email", changed);
        Assert.Contains("Bio", changed);

        repo.Verify(r => r.UpdateAsync(profile, It.IsAny<CancellationToken>()), Times.Once);
    }
}