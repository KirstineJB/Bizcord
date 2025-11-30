namespace ProfileService.Contracts.Messages
{
    public class ChargeUserPayment
    {
        public Guid UserId { get; }
        public decimal Amount { get; }

        public ChargeUserPayment(Guid userId, decimal amount)
        {
            UserId = userId;
            Amount = amount;
        }
    }
}
