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

    public bool IsEmailConfirmed { get; set; }
    public string? EmailConfirmationToken { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiry { get; set; }
    public string? AccountDeletionToken { get; set; }
    public DateTime? AccountDeletionTokenExpiry { get; set; }

    public string? AllowedAgencyIds { get; set; }
    public string? AllowedProjectIds { get; set; }
    public bool CanCreateProjects { get; set; }
    public bool CanCreateApiKeys { get; set; }

    // Navigation
    public Tenant Tenant { get; set; } = null!;
}
