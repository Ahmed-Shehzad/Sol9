using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace WebApplication2.Controllers;

[ApiController]
[Route("openapi")]
public sealed class OpenApiController : ControllerBase
{
    private readonly IOpenApiDocumentProvider _provider;

    public OpenApiController(IOpenApiDocumentProvider provider)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    [HttpGet("v1.yaml")]
    [Produces("application/yaml")]
    public async Task<IActionResult> GetYamlAsync(CancellationToken cancellationToken)
    {
        OpenApiDocument document = await _provider.GetOpenApiDocumentAsync(cancellationToken).ConfigureAwait(false);
        document.Servers = [new OpenApiServer { Url = $"{Request.Scheme}://{Request.Host}{Request.PathBase}" }];

        await using var textWriter = new StringWriter();
        var yamlWriter = new OpenApiYamlWriter(textWriter);
        document.SerializeAsV31(yamlWriter);

        return Content(textWriter.ToString(), "application/yaml");
    }
}
