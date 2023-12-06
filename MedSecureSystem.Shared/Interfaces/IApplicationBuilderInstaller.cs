
using Microsoft.AspNetCore.Builder;

namespace MedSecureSystem.Shared.Interfaces
{
    public interface IApplicationBuilderInstaller
    {
        void InstallApplicationBuilder(IApplicationBuilder app);
    }
}
