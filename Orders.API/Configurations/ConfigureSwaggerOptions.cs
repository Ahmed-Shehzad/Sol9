﻿using System.Reflection;
using Asp.Versioning.ApiExplorer;
using BuildingBlocks.Utilities.Filters.Swashbuckle;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Orders.API.Configurations;

/// <summary>
/// A class that configures Swagger options for an API.
/// </summary>
/// <param name="provider">An instance of <see cref="IApiVersionDescriptionProvider"/> to provide API version descriptions.</param>
public class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider, IConfiguration configuration) : IConfigureOptions<SwaggerGenOptions>
{
    /// <summary>
    /// Configures the Swagger options.
    /// </summary>
    /// <param name="options">The <see cref="SwaggerGenOptions"/> to configure.</param>
    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in provider.ApiVersionDescriptions)
        {
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

            if (File.Exists(xmlPath)) options.IncludeXmlComments(xmlPath);

            var info = CreateInfoForApiVersion(description);
            options.SwaggerDoc(description.GroupName, info);
        }
        options.DescribeAllParametersInCamelCase();
        options.SchemaFilter<UlidSchemaFilter>();
        options.SelectDiscriminatorNameUsing(_ => "discriminator");
        options.SelectSubTypesUsing(_ => null);

        var keycloak = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            OpenIdConnectUrl = new Uri(configuration["Keycloak:OpenIdConnectUrl"]!),
            Flows = new OpenApiOAuthFlows
            {
                Implicit = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri(configuration["Keycloak:AuthorizationEndpoint"]!),
                    Scopes = new Dictionary<string, string>
                    {
                        { "openid", "openid" },
                        { "profile", "profile" }
                    }
                }
            }
        };
        
        options.AddSecurityDefinition(nameof(keycloak), keycloak);
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Id = nameof(keycloak),
                        Type = ReferenceType.SecurityScheme,
                    },
                    In = ParameterLocation.Header,
                    Name = JwtBearerDefaults.AuthenticationScheme,
                    Scheme = JwtBearerDefaults.AuthenticationScheme,
                },
                Array.Empty<string>()
            }
        });
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
            Title = $"Orders API {description.GroupName}",
            Version = description.ApiVersion.ToString(),
            Description = $"Orders API {description.GroupName}",
        };
        
        if (description.IsDeprecated) info.Description += " This API version has been deprecated.";
        return info;
    }
}