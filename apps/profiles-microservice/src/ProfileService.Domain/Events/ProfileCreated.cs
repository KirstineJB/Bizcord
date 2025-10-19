
namespace ProfileService.Domain.Events
{
    public record ProfileCreated(Guid ProfileId, string Username, string DisplayName, string Email);
}
