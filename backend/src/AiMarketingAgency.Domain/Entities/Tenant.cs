using AiMarketingAgency.Domain.Enums;
using AiMarketingAgency.Domain.Interfaces;

namespace AiMarketingAgency.Domain.Entities;

public class Tenant : BaseEntity, ISoftDeletable
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public PlanTier Plan { get; set; } = PlanTier.FreeTrial;
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Agency> Agencies { get; set; } = new List<Agency>();
    public ICollection<LlmProviderKey> LlmProviderKeys { get; set; } = new List<LlmProviderKey>();
    public Subscription? Subscription { get; set; }
}
