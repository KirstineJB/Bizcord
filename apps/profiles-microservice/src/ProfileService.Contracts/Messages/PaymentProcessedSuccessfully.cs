namespace ProfileService.Contracts.Messages
{

    public class PaymentProcessedSuccessfully
    {
        public Guid UserId { get; }

        public PaymentProcessedSuccessfully(Guid userId)
        {
            UserId = userId;
        }
    }
}
