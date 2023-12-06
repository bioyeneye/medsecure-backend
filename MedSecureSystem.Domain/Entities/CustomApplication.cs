using OpenIddict.EntityFrameworkCore.Models;

namespace MedSecureSystem.Domain.Entities
{
    public class CustomApplication : OpenIddictEntityFrameworkCoreApplication<string, CustomAuthorization, CustomToken>
    {
        public bool IsActive { get; set; }
        public string BusinessKey { get; set; }
        public CustomApplication()
        {
            // Generate a new string identifier.
            Id = Guid.NewGuid().ToString();
        }
    }
}