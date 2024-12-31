using BuildingBlocks.Extensions.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BuildingBlocks.Utilities.Filters.Swashbuckle;

public static class SwashbuckleCustomizationExtensions
{
    /// <summary>
    /// Removes route and query parameters from model
    /// </summary>
    private class FixOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var parameters = operation.Parameters.Select(p => p.Name).ToArray();
            var schemaDescriptor = context
                .ApiDescription
                .ParameterDescriptions
                .FirstOrDefault(p => !parameters.Contains(p.Name, StringComparer.OrdinalIgnoreCase));
            if (schemaDescriptor == null) return;
            if (context.SchemaRepository.Schemas.TryGetValue(schemaDescriptor.Type.Name, out var schema))
            {
                schema.Properties.RemoveRange(schema.Properties.Where(p => 
                    parameters.Contains(p.Key, StringComparer.OrdinalIgnoreCase)));
            }
        }
    }
        
    public static void AddCustomizations(this SwaggerGenOptions options)
    {
        options.OperationFilter<FixOperationFilter>();
    }
}