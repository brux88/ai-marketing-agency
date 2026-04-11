using AiMarketingAgency.Domain.Interfaces;

namespace AiMarketingAgency.Domain.Entities;

public class NewsletterSubscriber : BaseEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid AgencyId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UnsubscribedAt { get; set; }

    // Navigation
    public Agency Agency { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
}
