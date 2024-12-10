using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BuildingBlocks.Utilities.Filters.Swashbuckle;

public class UlidSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type != typeof(Ulid)) return;
        
        schema.Type = "string";
        schema.Format = "ulid"; // Custom format for ULID
        schema.Example = new OpenApiString(Ulid.NewUlid().ToString()); // Example ULID
    }
}