using MedSecureSystem.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedSecureSystem.Domain.Entities
{
    public class DeliveryRequest : BaseEntity
    {
        [Required]
        public DeliveryRequestStatus Status { get; set; }

        [Required]
        public long BusinessId { get; set; }

        [Required]
        public string PatientId { get; set; }

        public string? DriverId { get; set; }
        public string? AgentId { get; set; }



        public string? CodeToGiveToDriver { get; set; } // agent
        public string? CodeToConfirmDelivery { get; set; } //  driver
        public string? CodeToConfirmReception { get; set; } // patient


        // Timestamps for business agent actions
        public DateTime? AgentAcceptedTime { get; set; } // Nullable for when the agent has not yet accepted
        public DateTime? AgentCompletedTime { get; set; } // Nullable for when the agent has not yet completed the request

        // Timestamps for driver actions
        public DateTime? DriverAcceptedTime { get; set; } // Nullable for when the driver has not yet accepted
        public DateTime? DriverStartedDeliveryTime { get; set; } // Nullable for when the driver has not yet started delivery
        public DateTime? DriverCompletedTime { get; set; } // Nullable for when the driver has not yet completed the delivery

        // Timestamp for when the patient received the request
        public DateTime? PatientReceivedTime { get; set; } // Nullable for when the patient has not yet received the request

        [ForeignKey("AgentId")]
        public virtual ApplicationUser? Agent { get; set; }
        [ForeignKey("PatientId")]
        public virtual ApplicationUser Patient { get; set; }
        [ForeignKey("DriverId")]
        public virtual ApplicationUser? Driver { get; set; }
        [ForeignKey("BusinessId")]
        public virtual Business Business { get; set; }

        public virtual List<DeliveryRequestItem> Items { get; set; }
        public virtual List<Feedback> Feedbacks { get; set; }

    }

}