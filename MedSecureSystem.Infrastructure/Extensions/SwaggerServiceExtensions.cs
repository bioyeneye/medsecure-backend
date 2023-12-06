using System.ComponentModel;
using System.Reflection;
using MedSecureSystem.Infrastructure.Filters;
using MedSecureSystem.Infrastructure.Options;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using MedSecureSystem.Shared.Extensions;

namespace MedSecureSystem.Infrastructure.Extensions;

public static class SwaggerServiceExtensions
{
    public static void AddSwaggerDocumentation(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSwaggerGen(c =>
        {
            // Configure Swagger for Bearer authentication
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            var serviceProvider = services.BuildServiceProvider();
            var openApiInfo = serviceProvider.GetRequiredService<IOptions<OpenApiInfoOptions>>();
            var provider = serviceProvider.GetRequiredService<IApiVersionDescriptionProvider>();

            // Iterate through the API versions and generate Swagger documentation for each version
            foreach (var description in provider.ApiVersionDescriptions)
            {
                c.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description, openApiInfo.Value));
            }

            c.OperationFilter<SwaggerDefaultValuesFilter>();

            var executingAssembly = Assembly.GetExecutingAssembly();
            var solutionAssemblies = executingAssembly.GetSolutionAssemblies(nameof(MedSecureSystem))?.ToList();

            if (solutionAssemblies == null || !solutionAssemblies.Any()) return;
            solutionAssemblies.ForEach(assembly =>
            {
                var xmlFile = $"{assembly.GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }
            });
        });
    }

    private static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description, OpenApiInfoOptions? options)
    {
        var info = new OpenApiInfo()
        {
            Title = $"{options?.Title} API {description.ApiVersion} {description.GroupName}",
            Version = description.ApiVersion.ToString(),
            Description = options?.Description ?? "A sample application with Swagger, Swashbuckle, and API versioning.",
            Contact = new OpenApiContact { Name = options?.Contact?.Name ?? "Oyeneye Bolaji", Email = options?.Contact?.Name ?? "b.oyeneye@devinsight.com" },
            TermsOfService = new Uri(options?.Contact?.Url ?? "https://opensource.org/licenses/MIT"),
            License = new OpenApiLicense() { Name = "MIT", Url = new Uri(options?.Contact?.Url ?? "https://opensource.org/licenses/MIT") }
        };

        if (description.IsDeprecated)
        {
            info.Description += " This API version has been deprecated.";
        }

        return info;
    }
}
