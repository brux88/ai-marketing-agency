using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Api.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
public class BillingController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly ITenantContext _tenantContext;
    private readonly IAppDbContext _context;
    private readonly IConfiguration _configuration;

    public BillingController(ISubscriptionService subscriptionService, ITenantContext tenantContext, IAppDbContext context, IConfiguration configuration)
    {
        _subscriptionService = subscriptionService;
        _tenantContext = tenantContext;
        _context = context;
        _configuration = configuration;
    }

    [HttpGet("usage")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<BillingUsageDto>>> GetUsage(CancellationToken ct)
    {
        var tenantId = _tenantContext.TenantId;
        var subscription = await _context.Subscriptions
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId, ct);

        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var periodStart = subscription?.CurrentPeriodEnd?.AddMonths(-1) ?? monthStart;

        var agencyCount = await _context.Agencies
            .IgnoreQueryFilters()
            .CountAsync(a => a.TenantId == tenantId && a.IsActive, ct);

        var jobsThisMonth = await _context.AgentJobs
            .IgnoreQueryFilters()
            .CountAsync(j => j.TenantId == tenantId && j.CreatedAt >= periodStart, ct);

        var plan = subscription?.PlanTier.ToString() ?? "FreeTrial";
        var maxAgencies = subscription?.MaxAgencies ?? 1;
        var maxJobs = subscription?.MaxJobsPerMonth ?? 50;
        var status = subscription?.Status.ToString() ?? "FreeTrial";

        return Ok(ApiResponse<BillingUsageDto>.Ok(new BillingUsageDto
        {
            Plan = plan,
            Status = status,
            AgenciesUsed = agencyCount,
            MaxAgencies = maxAgencies,
            JobsUsed = jobsThisMonth,
            MaxJobs = maxJobs,
            CurrentPeriodEnd = subscription?.CurrentPeriodEnd,
            TrialEndsAt = subscription?.TrialEndsAt
        }));
    }

    [HttpGet("prices")]
    [Authorize]
    public ActionResult<ApiResponse<object>> GetPrices()
    {
        return Ok(ApiResponse<object>.Ok(new
        {
            basic = _configuration["Stripe:PriceIds:Basic"] ?? "",
            pro = _configuration["Stripe:PriceIds:Pro"] ?? "",
            enterprise = _configuration["Stripe:PriceIds:Enterprise"] ?? "",
        }));
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

public class BillingUsageDto
{
    public string Plan { get; set; } = "FreeTrial";
    public string Status { get; set; } = "FreeTrial";
    public int AgenciesUsed { get; set; }
    public int MaxAgencies { get; set; }
    public int JobsUsed { get; set; }
    public int MaxJobs { get; set; }
    public DateTime? CurrentPeriodEnd { get; set; }
    public DateTime? TrialEndsAt { get; set; }
}

public record CreateCheckoutRequest(string PriceId, string SuccessUrl, string CancelUrl);
public record CreatePortalRequest(string ReturnUrl);
