using System.Security.Claims;
using AiMarketingAgency.Application.Common.Interfaces;

namespace AiMarketingAgency.Api.Middleware;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var tenantClaim = context.User.FindFirst("tenant_id")?.Value;
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (Guid.TryParse(tenantClaim, out var tenantId) && Guid.TryParse(userIdClaim, out var userId))
            {
                tenantContext.SetTenant(tenantId, userId);
            }
        }

        await _next(context);
    }
}
