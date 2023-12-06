using System.ComponentModel.DataAnnotations.Schema;

namespace MedSecureSystem.Domain.Entities
{
    public class Business : BaseEntity
    {
        public string BusinessUniqueKey { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Email { get; set; }
        public bool IsVerified { get; set; }
        public string VerificationToken { get; set; }

        [ForeignKey("BusinessCredential")]
        public long? BusinessCredentialId { get; set; }

        // Navigation property for the one-to-one relationship
        public virtual BusinessCredential BusinessCredential { get; set; }
    }
}
