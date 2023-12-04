using MedSecureSystem.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedSecureSystem.Domain.Entities
{
    public class Feedback : BaseEntity
    {
        public long DeliveryRequestId { get; set; }
        public string? UserId { get; set; } // ID of the user providing feedback
        public UserTypes UserType { get; set; } // Enum indicating whether the user is a Patient, Agent, or Driver
        public string Comments { get; set; }

        [ForeignKey("DeliveryRequestId")]
        public virtual DeliveryRequest? DeliveryRequest { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
    }

}