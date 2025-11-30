namespace ProfileService.Contracts;

public class UpgradeUserToPremium
{
    public Guid UserId { get; }

    public UpgradeUserToPremium(Guid userId)
    {
        UserId = userId;
    }
}