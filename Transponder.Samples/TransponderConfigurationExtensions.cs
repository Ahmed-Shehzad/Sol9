using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Transponder;

namespace Transponder.Samples;

public static class TransponderConfigurationExtensions
{
    public static TransponderSettings AddTransponderSettings(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "TransponderSettings")
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        IConfigurationSection section = configuration.GetSection(sectionName);

        services.AddOptions<TransponderSettings>()
            .Bind(section);
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<TransponderSettings>>().Value);

        return section.Get<TransponderSettings>() ?? new TransponderSettings();
    }
}
