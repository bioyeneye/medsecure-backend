using MedSecureSystem.Domain.Entities;
using MedSecureSystem.Domain.Interfaces;
using MedSecureSystem.Infrastructure.Data;
using MedSecureSystem.Shared.Attributes;
using MedSecureSystem.Shared.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MedSecureSystem.Infrastructure.Installers.ServiceConfigurations
{
    [Installer(position: 3)]
    public class DatabaseServiceInstaller : IServiceInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<MedSecureContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

                // Register the entity sets needed by OpenIddict.
                // Register the entity sets needed by OpenIddict but use the specified entities instead of the default ones.
                options.UseOpenIddict<CustomApplication, CustomAuthorization, CustomScope, CustomToken, string>();
            });

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<MedSecureContext>()
                .AddDefaultTokenProviders();

            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        }
    }
}
