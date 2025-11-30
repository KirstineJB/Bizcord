
using Rebus.Sagas;

namespace ProfileService.Api.Sagas
{
    public class UserUpgradeSagaData : ISagaData
    {
     
        public Guid Id { get; set; }          // Saga ID
        public int Revision { get; set; }    

        // Our correlation key:
        public Guid UserId { get; set; }

        // Flags success steps
        public bool PaymentProcessed { get; set; }
        public bool ProfileUpdated { get; set; }
        public bool NotificationSent { get; set; }

        // Husker orgiginalt navn for rollback hvis noget fejler
        public string? OriginalDisplayName { get; set; }
    }

}

   

