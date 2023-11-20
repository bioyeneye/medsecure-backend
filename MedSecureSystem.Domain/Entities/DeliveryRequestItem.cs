using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedSecureSystem.Domain.Entities
{
    public class DeliveryRequestItem : BaseEntity
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Quantity { get; set; }
        public string Note { get; set; }

        [Required]

        public long DeliveryRequestId { get; set; }

        [ForeignKey("DeliveryRequestId")]
        public virtual DeliveryRequest DeliveryRequest { get; set; }
    }
}
