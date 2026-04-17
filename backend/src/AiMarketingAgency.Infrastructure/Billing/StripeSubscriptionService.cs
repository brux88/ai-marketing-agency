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
        Guid tenantId, string priceId, string successUrl, string cancelUrl, CancellationToken ct = default)
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

        // If we already have a Stripe customer, reuse it
        if (subscription != null && !string.IsNullOrEmpty(subscription.StripeCustomerId))
        {
            sessionOptions.Customer = subscription.StripeCustomerId;
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

        var stripeEvent = EventUtility.ConstructEvent(json, signature, webhookSecret);

        _logger.LogInformation("Handling Stripe webhook event {EventType}", stripeEvent.Type);

        switch (stripeEvent.Type)
        {
            case EventTypes.CheckoutSessionCompleted:
                await HandleCheckoutSessionCompleted(stripeEvent, ct);
                break;

            case EventTypes.CustomerSubscriptionUpdated:
                await HandleSubscriptionUpdated(stripeEvent, ct);
                break;

            case EventTypes.CustomerSubscriptionDeleted:
                await HandleSubscriptionDeleted(stripeEvent, ct);
                break;

            case EventTypes.InvoicePaymentFailed:
                await HandleInvoicePaymentFailed(stripeEvent, ct);
                break;

            case EventTypes.InvoicePaymentSucceeded:
                await HandleInvoicePaymentSucceeded(stripeEvent, ct);
                break;

            default:
                _logger.LogInformation("Unhandled Stripe event type: {EventType}", stripeEvent.Type);
                break;
        }
    }

    private async Task HandleCheckoutSessionCompleted(Event stripeEvent, CancellationToken ct)
    {
        var session = stripeEvent.Data.Object as Session
            ?? throw new InvalidOperationException("Invalid checkout session event data.");

        if (!session.Metadata.TryGetValue("tenantId", out var tenantIdStr) ||
            !Guid.TryParse(tenantIdStr, out var tenantId))
        {
            _logger.LogWarning("Checkout session {SessionId} missing tenantId metadata", session.Id);
            return;
        }

        var subscription = await _context.Subscriptions
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId, ct);

        if (subscription == null)
        {
            // Create new subscription record
            subscription = new Domain.Entities.Subscription
            {
                TenantId = tenantId,
                StripeCustomerId = session.CustomerId ?? string.Empty,
                StripeSubscriptionId = session.SubscriptionId,
                Status = SubscriptionStatus.Active,
                PlanTier = PlanTier.Basic, // Will be updated by subscription.updated event
            };
            _context.Subscriptions.Add(subscription);
        }
        else
        {
            subscription.StripeCustomerId = session.CustomerId ?? subscription.StripeCustomerId;
            subscription.StripeSubscriptionId = session.SubscriptionId;
            subscription.Status = SubscriptionStatus.Active;
        }

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Checkout completed for tenant {TenantId}, subscription {SubscriptionId}",
            tenantId, session.SubscriptionId);
    }

    private async Task HandleSubscriptionUpdated(Event stripeEvent, CancellationToken ct)
    {
        var stripeSubscription = stripeEvent.Data.Object as Stripe.Subscription
            ?? throw new InvalidOperationException("Invalid subscription event data.");

        var subscription = await _context.Subscriptions
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.StripeSubscriptionId == stripeSubscription.Id, ct);

        if (subscription == null)
        {
            _logger.LogWarning("No local subscription found for Stripe subscription {SubscriptionId}", stripeSubscription.Id);
            return;
        }

        subscription.Status = MapStripeStatus(stripeSubscription.Status);
        subscription.CurrentPeriodEnd = stripeSubscription.CurrentPeriodEnd;

        // Update plan tier based on the price
        if (stripeSubscription.Items?.Data?.Count > 0)
        {
            var priceId = stripeSubscription.Items.Data[0].Price?.Id;
            subscription.PlanTier = MapPriceToPlanTier(priceId);
            UpdatePlanLimits(subscription);
        }

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Subscription updated for tenant {TenantId}: status={Status}, tier={Tier}",
            subscription.TenantId, subscription.Status, subscription.PlanTier);
    }

    private async Task HandleSubscriptionDeleted(Event stripeEvent, CancellationToken ct)
    {
        var stripeSubscription = stripeEvent.Data.Object as Stripe.Subscription
            ?? throw new InvalidOperationException("Invalid subscription event data.");

        var subscription = await _context.Subscriptions
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.StripeSubscriptionId == stripeSubscription.Id, ct);

        if (subscription == null)
        {
            _logger.LogWarning("No local subscription found for deleted Stripe subscription {SubscriptionId}", stripeSubscription.Id);
            return;
        }

        subscription.Status = SubscriptionStatus.Cancelled;
        subscription.PlanTier = PlanTier.FreeTrial;
        UpdatePlanLimits(subscription);

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Subscription cancelled for tenant {TenantId}",
            subscription.TenantId);
    }

    private async Task HandleInvoicePaymentFailed(Event stripeEvent, CancellationToken ct)
    {
        var invoice = stripeEvent.Data.Object as Invoice
            ?? throw new InvalidOperationException("Invalid invoice event data.");

        if (string.IsNullOrEmpty(invoice.SubscriptionId)) return;

        var subscription = await _context.Subscriptions
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.StripeSubscriptionId == invoice.SubscriptionId, ct);

        if (subscription == null) return;

        subscription.Status = SubscriptionStatus.PastDue;
        await _context.SaveChangesAsync(ct);

        _logger.LogWarning(
            "Payment failed for tenant {TenantId}, subscription {SubscriptionId}",
            subscription.TenantId, invoice.SubscriptionId);
    }

    private async Task HandleInvoicePaymentSucceeded(Event stripeEvent, CancellationToken ct)
    {
        var invoice = stripeEvent.Data.Object as Invoice
            ?? throw new InvalidOperationException("Invalid invoice event data.");

        if (string.IsNullOrEmpty(invoice.SubscriptionId)) return;

        var subscription = await _context.Subscriptions
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.StripeSubscriptionId == invoice.SubscriptionId, ct);

        if (subscription == null) return;

        subscription.Status = SubscriptionStatus.Active;
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Payment succeeded for tenant {TenantId}, subscription {SubscriptionId}",
            subscription.TenantId, invoice.SubscriptionId);
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
        (subscription.MaxAgencies, subscription.MaxJobsPerMonth) = subscription.PlanTier switch
        {
            PlanTier.FreeTrial => (1, 50),
            PlanTier.Basic => (3, 100),
            PlanTier.Pro => (10, 500),
            PlanTier.Enterprise => (50, 5000),
            _ => (1, 20)
        };
    }
}
