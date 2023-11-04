using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MedSecureSystem.Infrastructure.Filters
{
    public class ApiVersionDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            foreach (var apiDescription in context.ApiDescriptions)
            {
                var groupName = apiDescription.GroupName;
                if (string.IsNullOrEmpty(groupName))
                    continue;

                    
                var path = $"/{apiDescription.RelativePath.TrimEnd('/')}";
                if (swaggerDoc.Paths.ContainsKey(path))
                {
                    swaggerDoc.Paths[$"{path}"] = new OpenApiPathItem();
                }
            }
        }
        
    }
}
