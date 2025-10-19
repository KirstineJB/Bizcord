

namespace ProfileService.Domain.Events;

public record ProfileUpdated(Guid ProfileId, string DisplayName, string Email);