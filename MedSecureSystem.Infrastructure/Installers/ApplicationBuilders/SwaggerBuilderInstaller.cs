using MedSecureSystem.Shared.Attributes;
using MedSecureSystem.Shared.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MedSecureSystem.Infrastructure.Installers.ApplicationBuilders
{
    [Installer(position: 1)]
    public class SwaggerBuilderInstaller : IApplicationBuilderInstaller
    {
        public void InstallApplicationBuilder(IApplicationBuilder app)
        {
            var webHostEnvironment = app.ApplicationServices.GetService<IWebHostEnvironment>();

            if (webHostEnvironment == null || !webHostEnvironment.IsDevelopment()) return;

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                var provider =
                    app.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();

                // Build a swagger endpoint for each discovered API version
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    var url = $"/swagger/{description.GroupName}/swagger.json";
                    var name = $"MedSecure {description.GroupName.ToUpperInvariant()}";
                    options.SwaggerEndpoint(url, name);
                }
            });
        }
    }
}
