using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BuildingBlocks.Utilities.Types;

/// <summary>
/// A class that configures Swagger options for an API.
/// </summary>
/// <param name="provider">An instance of <see cref="IApiVersionDescriptionProvider"/> to provide API version descriptions.</param>
public class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) : IConfigureOptions<SwaggerGenOptions>
{
    /// <summary>
    /// Configures the Swagger options.
    /// </summary>
    /// <param name="options">The <see cref="SwaggerGenOptions"/> to configure.</param>
    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
        }
    }

    /// <summary>
    /// Creates an <see cref="OpenApiInfo"/> object for a specific API version.
    /// </summary>
    /// <param name="description">The <see cref="ApiVersionDescription"/> for which to create the info.</param>
    /// <returns>An <see cref="OpenApiInfo"/> object with the specified API version details.</returns>
    private static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
    {
        var info = new OpenApiInfo
        {
            Title = $"Identity API {description.GroupName}",
            Version = description.ApiVersion.ToString(),
            Description = $"Identity API {description.GroupName}"
        };
        return info;
    }
}