using AiMarketingAgency.Domain.Interfaces;

namespace AiMarketingAgency.Domain.Entities;

public class UsageRecord : BaseEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid? AgencyId { get; set; }
    public DateTime Period { get; set; }
    public int JobsCount { get; set; }
    public int TokensUsed { get; set; }
    public decimal EstimatedCost { get; set; }

    // Navigation
    public Tenant Tenant { get; set; } = null!;
    public Agency? Agency { get; set; }
}
