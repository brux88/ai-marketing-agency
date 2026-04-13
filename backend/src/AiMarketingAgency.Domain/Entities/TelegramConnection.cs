using AiMarketingAgency.Domain.Interfaces;

namespace AiMarketingAgency.Domain.Entities;

public class TelegramConnection : BaseEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid AgencyId { get; set; }
    public long ChatId { get; set; }
    public string? ChatTitle { get; set; }
    public string? Username { get; set; }
    public bool NotifyOnContentGenerated { get; set; } = true;
    public bool NotifyOnApprovalNeeded { get; set; } = true;
    public bool NotifyOnPublished { get; set; } = true;
    public bool AllowCommands { get; set; } = true;
    public bool IsActive { get; set; } = true;

    // Navigation
    public Agency Agency { get; set; } = null!;
}
