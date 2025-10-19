using Microsoft.Extensions.DependencyInjection;
using ProfileService.Contracts;
using ProfileService.Domain.Entities;
using ProfileService.Domain.Repositories;
using ProfileService.Domain.ValueObjects;
using ProfileService.IntegrationTests.Support;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace ProfileService.IntegrationTests;

public class PutProfileTests : IClassFixture<ProfilesApiFactory>
{
    private readonly ProfilesApiFactory _factory;

    public PutProfileTests(ProfilesApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Put_Profile_Updates_And_Publishes_Event()
    {
 
        var client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IUserProfileRepository>();

        var profile = UserProfile.Create("kirst", "Kirst Old", Email.Create("old@test.com"), "old-bio");
        await repo.AddAsync(profile, CancellationToken.None);

        var payload = new
        {
            displayName = "Kirst New",
            email = "new@test.com",
            bio = "new-bio",
            avatarUrl = "https://cdn/new.png"
        };

     
        var resp = await client.PutAsJsonAsync($"/api/v1/profiles/{profile.Id}", payload);


        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

        var shared = await resp.Content.ReadFromJsonAsync<ProfileSharedDto>();
        Assert.NotNull(shared);
        Assert.Equal("Kirst New", shared!.DisplayName);
        Assert.Equal("new@test.com", shared.Email);



        var updated = await repo.GetByIdAsync(profile.Id);
        Assert.NotNull(updated);
        Assert.Equal("Kirst New", updated!.DisplayName);
        Assert.Equal("new@test.com", updated.Email.Value);


        var published = _factory.FakeBus.Published.ToArray();
        Assert.Single(published);
        Assert.Equal("profiles.updated", published[0].Topic);
        Assert.IsType<ProfileUpdated>(published[0].Message);

        var evt = (ProfileUpdated)published[0].Message;
        Assert.Equal(profile.Id, evt.ProfileId);
        Assert.Equal("Kirst New", evt.DisplayName);
        Assert.Equal("new@test.com", evt.Email);

        Assert.Contains("DisplayName", evt.ChangedFields);
        Assert.Contains("Email", evt.ChangedFields);
        Assert.Contains("Bio", evt.ChangedFields);
    }
}