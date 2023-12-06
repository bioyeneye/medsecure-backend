using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MedSecureSystem.Infrastructure.Extensions;

public static class OptionsRegistrationExtensions
{
    public static void AddOptionsConfiguration<TOption>(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = nameof(TOption)) where TOption : class
    {
        var section = configuration.GetSection(sectionName);
        if (!section.Exists() && !section.GetChildren().Any())
        {
            throw new ArgumentException($"Configuration section '{sectionName}' not found.");
        }

        services.Configure<TOption>(section);

        /*services.AddOptions<TOption>().BindConfiguration(sectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();*/
    }
}
