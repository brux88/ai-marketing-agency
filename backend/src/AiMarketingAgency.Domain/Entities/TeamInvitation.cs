using AiMarketingAgency.Domain.Enums;
using AiMarketingAgency.Domain.Interfaces;

namespace AiMarketingAgency.Domain.Entities;

public class TeamInvitation : BaseEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Member;
    public Guid InvitedBy { get; set; }
    public string Token { get; set; } = string.Empty;
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
    public DateTime ExpiresAt { get; set; }
    public DateTime? AcceptedAt { get; set; }

    public string? AllowedAgencyIds { get; set; }
    public string? AllowedProjectIds { get; set; }
    public bool CanCreateProjects { get; set; }
    public bool CanCreateApiKeys { get; set; }

    // Navigation
    public Tenant Tenant { get; set; } = null!;
}
