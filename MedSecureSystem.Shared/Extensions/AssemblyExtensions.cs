using System.Reflection;

namespace MedSecureSystem.Shared.Extensions
{
    public static class AssemblyExtensions
    {
        public static IEnumerable<Assembly> GetSolutionAssemblies(this Assembly assembly, string solutionNamespace)
        {
            // Get all loaded assemblies
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            // Filter assemblies with namespaces matching the solution pattern
            var solutionAssemblies = loadedAssemblies.Where(a => a.FullName != null && a.FullName.StartsWith(solutionNamespace));

            return solutionAssemblies;
        }
    }
}
