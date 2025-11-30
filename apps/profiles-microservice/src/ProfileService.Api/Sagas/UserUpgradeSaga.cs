
using ProfileService.Contracts;
using ProfileService.Contracts.Messages;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Sagas;

namespace ProfileService.Api.Sagas;

public class UserUpgradeSaga :
    Saga<UserUpgradeSagaData>,
    IAmInitiatedBy<UpgradeUserToPremium>,
    IHandleMessages<PaymentProcessedSuccessfully>,
    IHandleMessages<PaymentFailed>,
    IHandleMessages<ProfileUpdatedSuccessfully>,
    IHandleMessages<ProfileUpdateFailed>,
    IHandleMessages<NotificationSentSuccessfully>,
    IHandleMessages<NotificationSendingFailed>
{
    private readonly IBus _bus;

    public UserUpgradeSaga(IBus bus)
    {
        _bus = bus;
    }

    protected override void CorrelateMessages(ICorrelationConfig<UserUpgradeSagaData> config)
    {
        // Every message has a UserId which we use as correlation key
        config.Correlate<UpgradeUserToPremium>(m => m.UserId, d => d.UserId);
        config.Correlate<PaymentProcessedSuccessfully>(m => m.UserId, d => d.UserId);
        config.Correlate<PaymentFailed>(m => m.UserId, d => d.UserId);
        config.Correlate<ProfileUpdatedSuccessfully>(m => m.UserId, d => d.UserId);
        config.Correlate<ProfileUpdateFailed>(m => m.UserId, d => d.UserId);
        config.Correlate<NotificationSentSuccessfully>(m => m.UserId, d => d.UserId);
        config.Correlate<NotificationSendingFailed>(m => m.UserId, d => d.UserId);
    }

    // 1) Start saga: 
    public async Task Handle(UpgradeUserToPremium message)
    {
        Console.WriteLine($"[Saga] Starting upgrade saga for user {message.UserId}");

        Data.UserId = message.UserId;

        // Step 1: ask payment service to charge user
        await _bus.Send(new ChargeUserPayment(message.UserId, amount: 99m));
    }

    // 2) Payment succeess
    public async Task Handle(PaymentProcessedSuccessfully message)
    {
        Console.WriteLine($"[Saga] Payment processed for user {message.UserId}");

        Data.PaymentProcessed = true;

        // Step 2: update profile to premium
        await _bus.Send(new UpdateUserProfileToPremium(message.UserId));
    }

    // 3) Payment failed 
    public Task Handle(PaymentFailed message)
    {
        Console.WriteLine($"[Saga] Payment failed for user {message.UserId}. Reason: {message.Reason}");

    
        MarkAsComplete();
        return Task.CompletedTask;
    }

    // 4) Profile updated check
    public async Task Handle(ProfileUpdatedSuccessfully message)
    {
        Console.WriteLine($"[Saga] Profile updated to premium for user {message.UserId}");

        Data.ProfileUpdated = true;
        Data.OriginalDisplayName = message.OriginalDisplayName;

        // Step 3: sender notification
        await _bus.Send(new SendUpgradeNotification(message.UserId));
    }

    // 5) Profile update failed -
    public async Task Handle(ProfileUpdateFailed message)
    {
        Console.WriteLine($"[Saga] Profile update FAILED for user {message.UserId}. Reason: {message.Reason}");

        if (Data.PaymentProcessed)
        {
            Console.WriteLine("[Saga] Return payment to user");
            await _bus.Send(new RefundUserPayment(message.UserId));
        }

        MarkAsComplete();
    }

    // 6) Notification succeeded -> saga complete
    public Task Handle(NotificationSentSuccessfully message)
    {
        Console.WriteLine($"[Saga] Notification sent for user {message.UserId}");

        Data.NotificationSent = true;
        MarkAsComplete();
        return Task.CompletedTask;
    }

    // 7) Notification failed -> compensate profile + payment
    public async Task Handle(NotificationSendingFailed message)
    {
        Console.WriteLine($"[Saga] Notification FAILED for user {message.UserId}. Reason: {message.Reason}");

        // If profile was updated, revert it
        if (Data.ProfileUpdated && Data.OriginalDisplayName is not null)
        {
            Console.WriteLine("[Saga] Compensating: reverting profile");
            await _bus.Send(new RevertUserProfileFromPremium(message.UserId, Data.OriginalDisplayName));
        }

        // If payment was processed, refund it
        if (Data.PaymentProcessed)
        {
            Console.WriteLine("[Saga] Compensating: refunding payment");
            await _bus.Send(new RefundUserPayment(message.UserId));
        }

        MarkAsComplete();
    }
}