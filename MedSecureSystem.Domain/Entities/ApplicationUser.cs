using Microsoft.AspNetCore.Identity;
using OpenIddict.EntityFrameworkCore.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedSecureSystem.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        // Additional properties if needed
        public string TenantKey { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Address { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    }

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

    public class CustomAuthorization : OpenIddictEntityFrameworkCoreAuthorization<string, CustomApplication, CustomToken>
    {
        public CustomAuthorization()
        {
            // Generate a new string identifier.
            Id = Guid.NewGuid().ToString();
        }
    }

    public class CustomScope : OpenIddictEntityFrameworkCoreScope<string>
    {
        public CustomScope()
        {
            // Generate a new string identifier.
            Id = Guid.NewGuid().ToString();
        }
    }

    public class CustomToken : OpenIddictEntityFrameworkCoreToken<string, CustomApplication, CustomAuthorization>
    {
        public CustomToken()
        {
            // Generate a new string identifier.
            Id = Guid.NewGuid().ToString();
        }
    }
}