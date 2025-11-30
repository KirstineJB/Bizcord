namespace ProfileService.Contracts.Messages
{
    public class NotificationSendingFailed
    {
        public Guid UserId { get; }
        public string Reason { get; }

        public NotificationSendingFailed(Guid userId, string reason)
        {
            UserId = userId;
            Reason = reason;
        }
    }
}
