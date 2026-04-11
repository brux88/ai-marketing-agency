using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiMarketingAgency.Api.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
public class BillingController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly ITenantContext _tenantContext;

    public BillingController(ISubscriptionService subscriptionService, ITenantContext tenantContext)
    {
        _subscriptionService = subscriptionService;
        _tenantContext = tenantContext;
    }

    [HttpPost("checkout-session")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> CreateCheckoutSession(
        [FromBody] CreateCheckoutRequest request, CancellationToken ct)
    {
        var sessionUrl = await _subscriptionService.CreateCheckoutSessionAsync(
            _tenantContext.TenantId, request.PriceId, request.SuccessUrl, request.CancelUrl, ct);
        return Ok(ApiResponse<object>.Ok(new { url = sessionUrl }));
    }

    [HttpPost("portal-session")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> CreatePortalSession(
        [FromBody] CreatePortalRequest request, CancellationToken ct)
    {
        var portalUrl = await _subscriptionService.CreatePortalSessionAsync(
            _tenantContext.TenantId, request.ReturnUrl, ct);
        return Ok(ApiResponse<object>.Ok(new { url = portalUrl }));
    }

    [HttpPost("webhooks/stripe")]
    public async Task<ActionResult> HandleStripeWebhook(CancellationToken ct)
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync(ct);
        var signature = Request.Headers["Stripe-Signature"].ToString();

        await _subscriptionService.HandleWebhookAsync(json, signature, ct);
        return Ok();
    }
}

public record CreateCheckoutRequest(string PriceId, string SuccessUrl, string CancelUrl);
public record CreatePortalRequest(string ReturnUrl);
