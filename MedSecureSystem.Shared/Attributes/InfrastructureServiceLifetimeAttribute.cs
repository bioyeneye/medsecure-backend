using Microsoft.Extensions.DependencyInjection;

namespace MedSecureSystem.Shared.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class InfrastructureServiceLifetimeAttribute : Attribute
    {
        public ServiceLifetime Lifetime { get; }

        public InfrastructureServiceLifetimeAttribute(ServiceLifetime lifetime)
        {
            Lifetime = lifetime;
        }
    }
}
