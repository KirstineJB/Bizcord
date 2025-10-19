using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfileService.Contracts
{
    /// <summary>
    /// Public-facing for user profiler for inter-service communication.
    /// Minimal fields; íngen internal details or behavior.
    /// </summary>
    public record ProfileSharedDto(
        Guid Id,
        string Username,
        string DisplayName,
        string Email,
        DateTimeOffset CreatedAt

    );
}
