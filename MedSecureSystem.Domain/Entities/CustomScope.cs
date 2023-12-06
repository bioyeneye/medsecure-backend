using OpenIddict.EntityFrameworkCore.Models;

namespace MedSecureSystem.Domain.Entities
{
    public class CustomScope : OpenIddictEntityFrameworkCoreScope<string>
    {
        public CustomScope()
        {
            // Generate a new string identifier.
            Id = Guid.NewGuid().ToString();
        }
    }
}