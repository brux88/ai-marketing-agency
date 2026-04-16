using AiMarketingAgency.Domain.Enums;
using AiMarketingAgency.Domain.Interfaces;

namespace AiMarketingAgency.Domain.Entities;

public class EmailConnector : BaseEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid AgencyId { get; set; }
    public Guid? ProjectId { get; set; }
    public EmailProviderType ProviderType { get; set; }
    public string? SmtpHost { get; set; }
    public int? SmtpPort { get; set; }
    public string? SmtpUsername { get; set; }
    public string? SmtpPassword { get; set; }
    public string? ApiKey { get; set; }
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    // Navigation
    public Agency Agency { get; set; } = null!;
    public Project? Project { get; set; }
    public Tenant Tenant { get; set; } = null!;
}
