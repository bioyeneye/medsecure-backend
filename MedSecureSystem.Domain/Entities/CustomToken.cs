using OpenIddict.EntityFrameworkCore.Models;

namespace MedSecureSystem.Domain.Entities
{
    public class CustomToken : OpenIddictEntityFrameworkCoreToken<string, CustomApplication, CustomAuthorization>
    {
        public CustomToken()
        {
            // Generate a new string identifier.
            Id = Guid.NewGuid().ToString();
        }
    }
}