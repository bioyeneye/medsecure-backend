using OpenIddict.EntityFrameworkCore.Models;

namespace MedSecureSystem.Domain.Entities
{
    public class CustomAuthorization : OpenIddictEntityFrameworkCoreAuthorization<string, CustomApplication, CustomToken>
    {
        public CustomAuthorization()
        {
            // Generate a new string identifier.
            Id = Guid.NewGuid().ToString();
        }
    }
}