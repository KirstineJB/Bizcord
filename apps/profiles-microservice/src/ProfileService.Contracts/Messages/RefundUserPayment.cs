namespace ProfileService.Contracts.Messages
{
    public class RefundUserPayment
    {
        public Guid UserId { get; }

        public RefundUserPayment(Guid userId)
        {
            UserId = userId;
        }
    }

    
    public class RevertUserProfileFromPremium
    {
        public Guid UserId { get; }
        public string OriginalDisplayName { get; }

        public RevertUserProfileFromPremium(Guid userId, string originalDisplayName)
        {
            UserId = userId;
            OriginalDisplayName = originalDisplayName;
        }
    }
}
