namespace ProfileService.Contracts
{
    public class UpdateUserProfileToPremium
    {
            public Guid UserId { get; }

            public UpdateUserProfileToPremium(Guid userId)
            {
                UserId = userId;
            }

    }
}
