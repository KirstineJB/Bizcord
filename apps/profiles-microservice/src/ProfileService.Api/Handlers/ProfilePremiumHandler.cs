
using ProfileService.Contracts;
using ProfileService.Contracts.Messages;
using ProfileService.Domain.Repositories;
using Rebus.Bus;
using Rebus.Handlers;

namespace ProfileService.Api.Handlers;

public class ProfilePremiumHandler :
    IHandleMessages<UpdateUserProfileToPremium>,
    IHandleMessages<RevertUserProfileFromPremium>
{
    private readonly IBus _bus;
    private readonly IUserProfileRepository _profileRepo;

    public ProfilePremiumHandler(IBus bus, IUserProfileRepository profileRepo)
    {
        _bus = bus;
        _profileRepo = profileRepo;
    }

    public async Task Handle(UpdateUserProfileToPremium message)
    {
        var profile = await _profileRepo.GetByIdAsync(message.UserId);

        if (profile is null)
        {
            await _bus.Send(new ProfileUpdateFailed(message.UserId, "Profile not found"));
            return;
        }

        // Folk med premium for tilføjet en stjerne og ordet Premium bagefter
        var originalDisplayName = profile.DisplayName;
        var newDisplayName = originalDisplayName + " * Premium";

        
        profile.Update(newDisplayName, profile.Email, profile.Bio);
        await _profileRepo.UpdateAsync(profile);

        await _bus.Send(new ProfileUpdatedSuccessfully(message.UserId, originalDisplayName));
    }

    public async Task Handle(RevertUserProfileFromPremium message)
    {
        var profile = await _profileRepo.GetByIdAsync(message.UserId);
        if (profile is null) return;

        // Tilføjer det gamle display navn
        profile.Update(message.OriginalDisplayName, profile.Email, profile.Bio);
        await _profileRepo.UpdateAsync(profile);
    }
}