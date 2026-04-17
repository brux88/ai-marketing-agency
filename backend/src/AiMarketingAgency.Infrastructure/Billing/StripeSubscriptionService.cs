using AiMarketingAgency.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;
using PlanTier = AiMarketingAgency.Domain.Enums.PlanTier;
using SubscriptionStatus = AiMarketingAgency.Domain.Enums.SubscriptionStatus;

namespace AiMarketingAgency.Infrastructure.Billing;

public class StripeSubscriptionService : ISubscriptionService
{
    private readonly IAppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripeSubscriptionService> _logger;

    public StripeSubscriptionService(
        IAppDbContext context,
        IConfiguration configuration,
        ILogger<StripeSubscriptionService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;

        StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
    }

    public async Task<string> CreateCheckoutSessionAsync(
        Guid tenantId, string priceId, string successUrl, string cancelUrl, string? customerEmail = null, CancellationToken ct = default)
    {
        var subscription = await _context.Subscriptions
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId, ct);

        var sessionOptions = new SessionCreateOptions
        {
            Mode = "subscription",
            LineItems =
            [
                new SessionLineItemOptions
                {
                    Price = priceId,
                    Quantity = 1,
                }
            ],
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            Metadata = new Dictionary<string, string>
            {
                ["tenantId"] = tenantId.ToString()
            }
        };

        if (subscription != null && !string.IsNullOrEmpty(subscription.StripeCustomerId)
            && subscription.StripeCustomerId.StartsWith("cus_"))
        {
            sessionOptions.Customer = subscription.StripeCustomerId;
        }
        else if (!string.IsNullOrEmpty(customerEmail))
        {
            sessionOptions.CustomerEmail = customerEmail;
        }

        var sessionService = new SessionService();
        var session = await sessionService.CreateAsync(sessionOptions, cancellationToken: ct);

        _logger.LogInformation(
            "Created Stripe checkout session {SessionId} for tenant {TenantId}",
            session.Id, tenantId);

        return session.Url!;
    }

    public async Task<string> CreatePortalSessionAsync(
        Guid tenantId, string returnUrl, CancellationToken ct = default)
    {
        var subscription = await _context.Subscriptions
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId, ct)
            ?? throw new InvalidOperationException("No subscription found for this tenant.");

        if (string.IsNullOrEmpty(subscription.StripeCustomerId))
            throw new InvalidOperationException("No Stripe customer associated with this tenant.");

        var portalOptions = new Stripe.BillingPortal.SessionCreateOptions
        {
            Customer = subscription.StripeCustomerId,
            ReturnUrl = returnUrl,
        };

        var portalService = new Stripe.BillingPortal.SessionService();
        var portalSession = await portalService.CreateAsync(portalOptions, cancellationToken: ct);

        _logger.LogInformation(
            "Created Stripe portal session for tenant {TenantId}, customer {CustomerId}",
            tenantId, subscription.StripeCustomerId);

        return portalSession.Url;
    }

    public async Task HandleWebhookAsync(string json, string signature, CancellationToken ct = default)
    {
        var webhookSecret = _configuration["Stripe:WebhookSecret"]
            ?? throw new InvalidOperationException("Stripe webhook secret is not configured.");

        var stripeEvent = EventUtility.ConstructEvent(json, signature, webhookSecret, throwOnApiVersionMismatch: false);

        // Parse raw JSON as fallback for cross-version compatibility
        var rawEvent = System.Text.Json.JsonDocument.Parse(json);

        _logger.LogInformation("Handling Stripe webhook event {EventType}", stripeEvent.Type);

        var dataObj = rawEvent.RootElement.GetProperty("data").GetProperty("object");

        switch (stripeEvent.Type)
        {
            case EventTypes.CheckoutSessionCompleted:
                await HandleCheckoutSessionCompleted(dataObj, ct);
                break;

            case EventTypes.CustomerSubscriptionUpdated:
                await HandleSubscriptionUpdated(dataObj, ct);
                break;

            case EventTypes.CustomerSubscriptionDeleted:
                await HandleSubscriptionDeleted(dataObj, ct);
                break;

            case EventTypes.InvoicePaymentFailed:
                await HandleInvoicePaymentFailed(dataObj, ct);
                break;

            case EventTypes.InvoicePaymentSucceeded:
                await HandleInvoicePaymentSucceeded(dataObj, ct);
                break;

            default:
                _logger.LogInformation("Unhandled Stripe event type: {EventType}", stripeEvent.Type);
                break;
        }

        rawEvent.Dispose();
    }

    private static string? GetStr(System.Text.Json.JsonElement el, string prop)
        => el.TryGetProperty(prop, out var v) && v.ValueKind == System.Text.Json.JsonValueKind.String ? v.GetString() : null;

    private async Task HandleCheckoutSessionCompleted(System.Text.Json.JsonElement data, CancellationToken ct)
    {
        var sessionId = GetStr(data, "id");
        var customerId = GetStr(data, "customer");
        var subscriptionId = GetStr(data, "subscription");

        string? tenantIdStr = null;
        if (data.TryGetProperty("metadata", out var meta) && meta.TryGetProperty("tenantId", out var tid))
            tenantIdStr = tid.GetString();

        _logger.LogInformation("CheckoutSessionCompleted: session={SessionId}, customer={CustomerId}, subscription={SubId}, tenantId={TenantId}",
            sessionId, customerId, subscriptionId, tenantIdStr);

        if (string.IsNullOrEmpty(tenantIdStr) || !Guid.TryParse(tenantIdStr, out var tenantId))
        {
            _logger.LogWarning("Checkout session {SessionId} missing tenantId metadata", sessionId);
            return;
        }

        var subscription = await _context.Subscriptions
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId, ct);

        if (subscription == null)
        {
            subscription = new Domain.Entities.Subscription
            {
                TenantId = tenantId,
                StripeCustomerId = customerId ?? string.Empty,
                StripeSubscriptionId = subscriptionId,
                Status = SubscriptionStatus.Active,
                PlanTier = PlanTier.Basic,
            };
            _context.Subscriptions.Add(subscription);
        }
        else
        {
            subscription.StripeCustomerId = customerId ?? subscription.StripeCustomerId;
            subscription.StripeSubscriptionId = subscriptionId;
            subscription.Status = SubscriptionStatus.Active;
        }

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Checkout completed for tenant {TenantId}, subscription {SubscriptionId}", tenantId, subscriptionId);
    }

    private async Task HandleSubscriptionUpdated(System.Text.Json.JsonElement data, CancellationToken ct)
    {
        var stripeSubId = GetStr(data, "id");
        var status = GetStr(data, "status");

        string? priceId = null;
        if (data.TryGetProperty("items", out var items) &&
            items.TryGetProperty("data", out var itemsData) &&
            itemsData.GetArrayLength() > 0)
        {
            var firstItem = itemsData[0];
            if (firstItem.TryGetProperty("price", out var price))
                priceId = GetStr(price, "id");
        }

        DateTime? currentPeriodEnd = null;
        if (data.TryGetProperty("current_period_end", out var cpe) && cpe.ValueKind == System.Text.Json.JsonValueKind.Number)
            currentPeriodEnd = DateTimeOffset.FromUnixTimeSeconds(cpe.GetInt64()).UtcDateTime;

        _logger.LogInformation("SubscriptionUpdated: id={SubId}, status={Status}, priceId={PriceId}", stripeSubId, status, priceId);

        var subscription = await _context.Subscriptions
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.StripeSubscriptionId == stripeSubId, ct);

        if (subscription == null)
        {
            _logger.LogWarning("No local subscription found for Stripe subscription {SubscriptionId}", stripeSubId);
            return;
        }

        subscription.Status = MapStripeStatus(status ?? "active");
        if (currentPeriodEnd.HasValue)
            subscription.CurrentPeriodEnd = currentPeriodEnd.Value;

        if (!string.IsNullOrEmpty(priceId))
        {
            subscription.PlanTier = MapPriceToPlanTier(priceId);
            UpdatePlanLimits(subscription);
        }

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Subscription updated for tenant {TenantId}: status={Status}, tier={Tier}",
            subscription.TenantId, subscription.Status, subscription.PlanTier);
    }

    private async Task HandleSubscriptionDeleted(System.Text.Json.JsonElement data, CancellationToken ct)
    {
        var stripeSubId = GetStr(data, "id");
        if (string.IsNullOrEmpty(stripeSubId)) return;

        var subscription = await _context.Subscriptions
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.StripeSubscriptionId == stripeSubId, ct);

        if (subscription == null)
        {
            _logger.LogWarning("No local subscription found for deleted Stripe subscription {SubscriptionId}", stripeSubId);
            return;
        }

        subscription.Status = SubscriptionStatus.Cancelled;
        subscription.PlanTier = PlanTier.FreeTrial;
        UpdatePlanLimits(subscription);

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Subscription cancelled for tenant {TenantId}", subscription.TenantId);
    }

    private async Task HandleInvoicePaymentFailed(System.Text.Json.JsonElement data, CancellationToken ct)
    {
        var subscriptionId = GetStr(data, "subscription");
        if (string.IsNullOrEmpty(subscriptionId)) return;

        var subscription = await _context.Subscriptions
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.StripeSubscriptionId == subscriptionId, ct);

        if (subscription == null) return;

        subscription.Status = SubscriptionStatus.PastDue;
        await _context.SaveChangesAsync(ct);

        _logger.LogWarning("Payment failed for tenant {TenantId}, subscription {SubscriptionId}",
            subscription.TenantId, subscriptionId);
    }

    private async Task HandleInvoicePaymentSucceeded(System.Text.Json.JsonElement data, CancellationToken ct)
    {
        var subscriptionId = GetStr(data, "subscription");
        if (string.IsNullOrEmpty(subscriptionId)) return;

        var subscription = await _context.Subscriptions
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.StripeSubscriptionId == subscriptionId, ct);

        if (subscription == null) return;

        subscription.Status = SubscriptionStatus.Active;
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Payment succeeded for tenant {TenantId}, subscription {SubscriptionId}",
            subscription.TenantId, subscriptionId);
    }

    private static SubscriptionStatus MapStripeStatus(string stripeStatus) => stripeStatus switch
    {
        "active" => SubscriptionStatus.Active,
        "past_due" => SubscriptionStatus.PastDue,
        "canceled" => SubscriptionStatus.Cancelled,
        "trialing" => SubscriptionStatus.Trialing,
        "unpaid" => SubscriptionStatus.PastDue,
        _ => SubscriptionStatus.Active
    };

    private PlanTier MapPriceToPlanTier(string? priceId)
    {
        if (string.IsNullOrEmpty(priceId)) return PlanTier.FreeTrial;

        var basicPriceId = _configuration["Stripe:PriceIds:Basic"];
        var proPriceId = _configuration["Stripe:PriceIds:Pro"];
        var enterprisePriceId = _configuration["Stripe:PriceIds:Enterprise"];

        if (!string.IsNullOrEmpty(enterprisePriceId) && priceId == enterprisePriceId)
            return PlanTier.Enterprise;
        if (!string.IsNullOrEmpty(proPriceId) && priceId == proPriceId)
            return PlanTier.Pro;
        if (!string.IsNullOrEmpty(basicPriceId) && priceId == basicPriceId)
            return PlanTier.Basic;

        if (priceId.Contains("enterprise", StringComparison.OrdinalIgnoreCase))
            return PlanTier.Enterprise;
        if (priceId.Contains("pro", StringComparison.OrdinalIgnoreCase))
            return PlanTier.Pro;
        if (priceId.Contains("basic", StringComparison.OrdinalIgnoreCase))
            return PlanTier.Basic;

        return PlanTier.Basic;
    }

    private static void UpdatePlanLimits(Domain.Entities.Subscription subscription)
    {
        (subscription.MaxAgencies, subscription.MaxProjects, subscription.MaxJobsPerMonth) = subscription.PlanTier switch
        {
            PlanTier.FreeTrial => (1, 3, 50),
            PlanTier.Basic => (3, 10, 100),
            PlanTier.Pro => (10, 50, 500),
            PlanTier.Enterprise => (50, 200, 5000),
            _ => (1, 3, 20)
        };
    }
}
