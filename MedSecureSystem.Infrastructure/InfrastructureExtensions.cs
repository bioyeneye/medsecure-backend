using System.Reflection;
using MedSecureSystem.Infrastructure.Extensions;
using MedSecureSystem.Infrastructure.Options;
using MedSecureSystem.Shared.Attributes;
using MedSecureSystem.Shared.Constants;
using MedSecureSystem.Shared.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MedSecureSystem.Shared.Extensions;
using MedSecureSystem.Application.Interfaces;
using MedSecureSystem.Infrastructure.Services;

namespace MedSecureSystem.Infrastructure
{
    public static class InfrastructureExtensions
    {
        public static void RegisterInfrastructureService(this IServiceCollection serviceCollection,
            IConfiguration configuration)
        {
            RegisterOptions(serviceCollection, configuration);
            RegisterServiceInstallers(serviceCollection, configuration);
            serviceCollection.AddInfrastructureServices();
        }

        public static void RegisterInfrastructureApplicationBuilder(this IApplicationBuilder app)
        {
            RegisterApplicationBuilderInstallers(app);
        }

        #region Service Configuration Methods

        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            var assembly = Assembly.GetAssembly(typeof(InfrastructureExtensions));
            var types = assembly?.GetTypes()?
                .Where(x => x.GetCustomAttributes<InfrastructureServiceLifetimeAttribute>().Any()) ?? Enumerable.Empty<Type>();

            foreach (var type in types)
            {
                var attribute = type.GetCustomAttribute<InfrastructureServiceLifetimeAttribute>();
                var interfaces = type.GetInterfaces();

                foreach (var interfaceType in interfaces)
                {
                    switch (attribute.Lifetime)
                    {
                        case ServiceLifetime.Singleton:
                            services.AddSingleton(interfaceType, type);
                            break;
                        case ServiceLifetime.Scoped:
                            services.AddScoped(interfaceType, type);
                            break;
                        case ServiceLifetime.Transient:
                            services.AddTransient(interfaceType, type);
                            break;
                    }
                }
            }

            return services;
        }


        private static void RegisterOptions(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddOptionsConfiguration<JwtTokenOptions>(configuration, SectionNames.JwtSectionName);
            serviceCollection.AddOptionsConfiguration<OpenApiInfoOptions>(configuration, SectionNames.OpenApiSectionName);
            serviceCollection.AddOptionsConfiguration<EmailSettings>(configuration, nameof(EmailSettings));
        }

        private static void RegisterServiceInstallers(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            var installers = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => typeof(IServiceInstaller)
                    .IsAssignableFrom(type) && type is { IsInterface: false, IsAbstract: false })
                .OrderBy(type => type.GetAttributeValue((InstallerAttribute attr) => attr.Position, 0))
                .Select(Activator.CreateInstance)
                .Cast<IServiceInstaller>()
                .ToList();

            installers.ForEach(installer => installer.InstallServices(serviceCollection, configuration));
        }

        #endregion

        #region Application Builder Methods

        private static void RegisterApplicationBuilderInstallers(IApplicationBuilder app)
        {
            var installers = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => typeof(IApplicationBuilderInstaller)
                    .IsAssignableFrom(type) && type is { IsInterface: false, IsAbstract: false })
                .OrderBy(type => type.GetAttributeValue((InstallerAttribute attr) => attr.Position, 0))
                .Select(Activator.CreateInstance)
                .Cast<IApplicationBuilderInstaller>()
                .ToList();

            installers.ForEach(installer => installer.InstallApplicationBuilder(app));
        }

        #endregion
    }
}
