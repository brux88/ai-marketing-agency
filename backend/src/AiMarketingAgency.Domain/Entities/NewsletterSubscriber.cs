using AiMarketingAgency.Domain.Interfaces;

namespace AiMarketingAgency.Domain.Entities;

public class NewsletterSubscriber : BaseEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid AgencyId { get; set; }
    public Guid? ProjectId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UnsubscribedAt { get; set; }

    // Opaque token used in the unsubscribe link so anyone with the newsletter
    // email can opt out, but random actors cannot unsubscribe by guessing
    // other people's addresses.
    public Guid UnsubscribeToken { get; set; } = Guid.NewGuid();

    // Navigation
    public Agency Agency { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
    public Project? Project { get; set; }
}
