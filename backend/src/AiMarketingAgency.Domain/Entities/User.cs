using AiMarketingAgency.Domain.Enums;
using AiMarketingAgency.Domain.Interfaces;

namespace AiMarketingAgency.Domain.Entities;

public class User : BaseEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Member;

    public string? AllowedAgencyIds { get; set; }
    public string? AllowedProjectIds { get; set; }
    public bool CanCreateProjects { get; set; }
    public bool CanCreateApiKeys { get; set; }

    // Navigation
    public Tenant Tenant { get; set; } = null!;
}
