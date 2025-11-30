namespace ProfileService.Contracts.Messages
{
    public class ProfileUpdatedSuccessfully
    {
        public Guid UserId { get; }
        public string OriginalDisplayName { get; }

        public ProfileUpdatedSuccessfully(Guid userId, string originalDisplayName)
        {
            UserId = userId;
            OriginalDisplayName = originalDisplayName;
        }
    }
}
