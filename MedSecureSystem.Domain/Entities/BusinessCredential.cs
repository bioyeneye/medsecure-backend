using System.ComponentModel.DataAnnotations.Schema;

namespace MedSecureSystem.Domain.Entities
{
    public class BusinessCredential : BaseEntity
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }

        [ForeignKey("BusinessCredential")]
        public long BusinessId { get; set; }

        public Business Business { get; set; }
    }
}