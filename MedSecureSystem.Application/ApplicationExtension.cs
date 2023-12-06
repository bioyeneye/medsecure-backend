using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedSecureSystem.Application
{
    public static class ApplicationExtension
    {
        public static IServiceCollection RegisterApplicationService(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(Mappings.MappingProfile));
            return services;
        }
    }
}
