namespace ProfileService.Contracts.Messages
{


    public class PaymentFailed
    {
        public Guid UserId { get; }
        public string Reason { get; }

        public PaymentFailed(Guid userId, string reason)
        {
            UserId = userId;
            Reason = reason;
        }
    }
}
