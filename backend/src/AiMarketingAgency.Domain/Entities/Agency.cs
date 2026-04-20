using AiMarketingAgency.Domain.Enums;
using AiMarketingAgency.Domain.Interfaces;
using AiMarketingAgency.Domain.ValueObjects;

namespace AiMarketingAgency.Domain.Entities;

public class Agency : BaseEntity, ITenantScoped, ISoftDeletable
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? LogoUrl { get; set; }
    public BrandVoice BrandVoice { get; set; } = new();
    public TargetAudience TargetAudience { get; set; } = new();
    public Guid? DefaultLlmProviderKeyId { get; set; }
    public ApprovalMode ApprovalMode { get; set; } = ApprovalMode.Manual;
    public int AutoApproveMinScore { get; set; } = 7;
    public bool AutoScheduleOnApproval { get; set; } = true;
    public Guid? ImageLlmProviderKeyId { get; set; }
    public Guid? VideoLlmProviderKeyId { get; set; }
    public bool EnableLogoOverlay { get; set; }
    public int LogoOverlayPosition { get; set; } = 3; // BottomRight
    public int LogoOverlayMode { get; set; } // 0=Overlay, 1=Banner
    public string? BrandBannerColor { get; set; } // hex like "#003366"
    public string? TelegramBotToken { get; set; }
    public string? TelegramBotUsername { get; set; }
    // Master kill switch for all Telegram notifications on this agency.
    public bool TelegramNotificationsEnabled { get; set; } = true;

    // Notifications fired when someone subscribes to the agency newsletter.
    public bool NotifyEmailOnSubscribed { get; set; } = true;
    public bool NotifyPushOnSubscribed { get; set; } = true;
    public bool NotifyTelegramOnSubscribed { get; set; } = true;
    public string? NotificationEmail { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation
    public Tenant Tenant { get; set; } = null!;
    public LlmProviderKey? DefaultLlmProviderKey { get; set; }
    public LlmProviderKey? ImageLlmProviderKey { get; set; }
    public LlmProviderKey? VideoLlmProviderKey { get; set; }
    public ICollection<Project> Projects { get; set; } = new List<Project>();
    public ICollection<ContentSource> ContentSources { get; set; } = new List<ContentSource>();
    public ICollection<AgentJob> AgentJobs { get; set; } = new List<AgentJob>();
    public ICollection<GeneratedContent> GeneratedContents { get; set; } = new List<GeneratedContent>();
    public ICollection<EditorialCalendarEntry> CalendarEntries { get; set; } = new List<EditorialCalendarEntry>();
    public ICollection<ContentSchedule> Schedules { get; set; } = new List<ContentSchedule>();
}
