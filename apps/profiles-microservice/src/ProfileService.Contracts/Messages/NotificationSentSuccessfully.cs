namespace ProfileService.Contracts.Messages
{

    public class NotificationSentSuccessfully
    {
        public Guid UserId { get; }

        public NotificationSentSuccessfully(Guid userId)
        {
            UserId = userId;
        }
    }
}
