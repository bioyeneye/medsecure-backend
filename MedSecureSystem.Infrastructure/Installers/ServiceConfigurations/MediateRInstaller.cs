using MedSecureSystem.Shared.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MedSecureSystem.Infrastructure.Installers.ServiceConfigurations;

public class MediateRInstaller : IServiceInstaller
{
    public void InstallServices(IServiceCollection services, IConfiguration configuration)
    {
        // services.AddMediatR(typeof(MediateRInstaller).Assembly); // Register MediatR
        RegisterHandlers(services);
    }

    private static void RegisterHandlers(IServiceCollection services)
    {

    }
}
