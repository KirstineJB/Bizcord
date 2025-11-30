namespace ProfileService.Contracts.Messages
{
    public class ProfileUpdateFailed
    {
        public Guid UserId { get; }
        public string Reason { get; }

        public ProfileUpdateFailed(Guid userId, string reason)
        {
            UserId = userId;
            Reason = reason;
        }
    }
}
