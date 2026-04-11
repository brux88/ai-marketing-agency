using AiMarketingAgency.Domain.Enums;
using AiMarketingAgency.Domain.Interfaces;

namespace AiMarketingAgency.Domain.Entities;

public class EditorialCalendarEntry : BaseEntity, ITenantScoped
{
    public Guid AgencyId { get; set; }
    public Guid TenantId { get; set; }
    public Guid ContentId { get; set; }
    public SocialPlatform? Platform { get; set; }
    public DateTime ScheduledAt { get; set; }
    public CalendarEntryStatus Status { get; set; } = CalendarEntryStatus.Draft;
    public DateTime? PublishedAt { get; set; }

    // Navigation
    public Agency Agency { get; set; } = null!;
    public GeneratedContent Content { get; set; } = null!;
}
