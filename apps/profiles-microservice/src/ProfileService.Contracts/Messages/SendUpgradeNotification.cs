namespace ProfileService.Contracts.Messages
{
    public class SendUpgradeNotification
    {
        public Guid UserId { get; }

        public SendUpgradeNotification(Guid userId)
        {
            UserId = userId;
        }
    }
}
