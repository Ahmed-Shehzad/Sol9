using BuildingBlocks.Contracts.Services.Tenants;
using BuildingBlocks.Utilities.Exceptions;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Utilities.Middlewares;

public class TenantMiddleware(ITenantService tenantService) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.Request.Headers.TryGetValue("X-TenantId", out var tenantId))
        {
            if (string.IsNullOrEmpty(tenantId))
            {
                throw new NotFoundException("Invalid or unauthorized tenant.");
            }

            if (Guid.TryParse(tenantId, out var tenantIdGuidValue))
            {
                if (!tenantIdGuidValue.Equals(Guid.Empty))
                {
                    tenantService.SetTenantId(new Ulid(tenantIdGuidValue));
                }
            }
            else if (Ulid.TryParse(tenantId, out var tenantIdValue))
            {
                tenantService.SetTenantId(tenantIdValue);
            }
        }

        await next(context);
    }
}