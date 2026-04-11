using AiMarketingAgency.Domain.Enums;
using AiMarketingAgency.Domain.Interfaces;

namespace AiMarketingAgency.Domain.Entities;

public class Subscription : BaseEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public string StripeCustomerId { get; set; } = string.Empty;
    public string? StripeSubscriptionId { get; set; }
    public PlanTier PlanTier { get; set; } = PlanTier.FreeTrial;
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Trialing;
    public DateTime? CurrentPeriodEnd { get; set; }
    public DateTime? TrialEndsAt { get; set; }
    public int MaxAgencies { get; set; } = 1;
    public int MaxJobsPerMonth { get; set; } = 20;

    // Navigation
    public Tenant Tenant { get; set; } = null!;
}
