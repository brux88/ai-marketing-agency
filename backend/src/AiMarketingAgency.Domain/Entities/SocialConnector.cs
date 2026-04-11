using AiMarketingAgency.Domain.Enums;
using AiMarketingAgency.Domain.Interfaces;

namespace AiMarketingAgency.Domain.Entities;

public class SocialConnector : BaseEntity, ITenantScoped, ISoftDeletable
{
    public Guid TenantId { get; set; }
    public Guid AgencyId { get; set; }
    public SocialPlatform Platform { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public string? AccountId { get; set; }
    public string? AccountName { get; set; }
    public string? ProfileImageUrl { get; set; }
    public DateTime? TokenExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public Agency Agency { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
}
