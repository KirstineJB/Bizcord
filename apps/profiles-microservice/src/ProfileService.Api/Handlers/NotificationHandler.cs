using ProfileService.Contracts.Messages;
using Rebus.Bus;
using Rebus.Handlers;


namespace ProfileService.Api.Handlers
{
   


    public class NotificationHandler :
        IHandleMessages<SendUpgradeNotification>
    {
        private readonly IBus _bus;

        public NotificationHandler(IBus bus)
        {
            _bus = bus;
        }

        public async Task Handle(SendUpgradeNotification message)
        {
            Console.WriteLine($"[Notification] Sending premium upgrade notification to user {message.UserId}...");

            // Fake success igen
            var success = true;

            if (success)
            {
                await _bus.Send(new NotificationSentSuccessfully(message.UserId));
            }
            else
            {
                await _bus.Send(new NotificationSendingFailed(message.UserId, "Notification service down"));
            }
        }
    }
}
