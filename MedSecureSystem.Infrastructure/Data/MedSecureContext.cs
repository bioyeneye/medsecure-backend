using MedSecureSystem.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MedSecureSystem.Infrastructure.Data
{
    public class MedSecureContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Business> Businesses { get; set; }
        public DbSet<BusinessCredential> BusinessCredentials { get; set; }
        public DbSet<DeliveryRequest> DeliveryRequests { get; set; }
        public DbSet<DeliveryRequestItem> DeliveryRequestItems { get; set; }


        public MedSecureContext(DbContextOptions<MedSecureContext> options) : base(options)
        {
        }
    }

}
