using AiMarketingAgency.Domain.Enums;
using AiMarketingAgency.Domain.Interfaces;
using AiMarketingAgency.Domain.ValueObjects;

namespace AiMarketingAgency.Domain.Entities;

public class Project : BaseEntity, ITenantScoped, ISoftDeletable
{
    public Guid AgencyId { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? LogoUrl { get; set; }
    public BrandVoice BrandVoice { get; set; } = new();
    public TargetAudience TargetAudience { get; set; } = new();
    public bool IsActive { get; set; } = true;

    // Optional per-project Telegram bot (overrides agency bot when set)
    public string? TelegramBotToken { get; set; }
    public string? TelegramBotUsername { get; set; }

    // Project-specific prompt templates (override agent defaults if set)
    public string? BlogPromptTemplate { get; set; }
    public string? SocialPromptTemplate { get; set; }
    public string? NewsletterPromptTemplate { get; set; }

    // Extracted site context (JSON with topics, audience, tone, guides, key pages)
    public string? ExtractedContext { get; set; }
    public DateTime? ExtractedContextAt { get; set; }

    // Per-project overrides for approval flow (fall back to agency values when null)
    public ApprovalMode? ApprovalMode { get; set; }
    public int? AutoApproveMinScore { get; set; }
    public bool? AutoScheduleOnApproval { get; set; }

    // Per-project logo overlay override (fall back to agency when EnableLogoOverlay is null)
    public bool? EnableLogoOverlay { get; set; }
    public int? LogoOverlayPosition { get; set; }
    public int? LogoOverlayMode { get; set; }
    public string? BrandBannerColor { get; set; }

    // Comma-separated list of enabled SocialPlatform names ("Twitter,LinkedIn"). Null = all.
    public string? EnabledSocialPlatforms { get; set; }

    // Email notification settings
    public bool NotifyEmailOnGeneration { get; set; }
    public bool NotifyEmailOnPublication { get; set; }
    public bool NotifyEmailOnApprovalNeeded { get; set; }
    public string? NotificationEmail { get; set; }

    // Push notification settings (mobile app via FCM)
    public bool NotifyPushOnGeneration { get; set; }
    public bool NotifyPushOnPublication { get; set; }
    public bool NotifyPushOnApprovalNeeded { get; set; }

    // Notifications fired when someone subscribes to the project newsletter.
    public bool NotifyEmailOnSubscribed { get; set; } = true;
    public bool NotifyPushOnSubscribed { get; set; } = true;
    public bool NotifyTelegramOnSubscribed { get; set; } = true;

    // Navigation
    public Agency Agency { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
    public ICollection<ContentSource> ContentSources { get; set; } = new List<ContentSource>();
    public ICollection<GeneratedContent> GeneratedContents { get; set; } = new List<GeneratedContent>();
    public ICollection<AgentJob> AgentJobs { get; set; } = new List<AgentJob>();
}
