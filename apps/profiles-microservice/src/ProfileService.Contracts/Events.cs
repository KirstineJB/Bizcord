using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfileService.Contracts
{
    public record ProfileCreated(
     Guid ProfileId,
     string Username,
     string DisplayName,
     string Email,
     DateTimeOffset CreatedAt
 );

    public record ProfileUpdated(
        Guid ProfileId,
        string DisplayName,
        string Email,
        DateTimeOffset UpdatedAt
    );
}
