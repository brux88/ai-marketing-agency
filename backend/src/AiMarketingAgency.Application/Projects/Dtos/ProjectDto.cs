using AiMarketingAgency.Domain.Enums;
using AiMarketingAgency.Domain.ValueObjects;

namespace AiMarketingAgency.Application.Projects.Dtos;

public class ProjectDto
{
    public Guid Id { get; set; }
    public Guid AgencyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? LogoUrl { get; set; }
    public BrandVoice BrandVoice { get; set; } = new();
    public TargetAudience TargetAudience { get; set; } = new();
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ContentSourcesCount { get; set; }
    public int GeneratedContentsCount { get; set; }

    public string? BlogPromptTemplate { get; set; }
    public string? SocialPromptTemplate { get; set; }
    public string? NewsletterPromptTemplate { get; set; }
    public string? ExtractedContext { get; set; }
    public DateTime? ExtractedContextAt { get; set; }

    public ApprovalMode? ApprovalMode { get; set; }
    public int? AutoApproveMinScore { get; set; }
    public bool? AutoScheduleOnApproval { get; set; }

    public bool? EnableLogoOverlay { get; set; }
    public int? LogoOverlayPosition { get; set; }
    public int? LogoOverlayMode { get; set; }
    public string? BrandBannerColor { get; set; }
    public string? EnabledSocialPlatforms { get; set; }

    public bool NotifyEmailOnGeneration { get; set; }
    public bool NotifyEmailOnPublication { get; set; }
    public bool NotifyEmailOnApprovalNeeded { get; set; }
    public string? NotificationEmail { get; set; }

    public bool NotifyPushOnGeneration { get; set; }
    public bool NotifyPushOnPublication { get; set; }
    public bool NotifyPushOnApprovalNeeded { get; set; }

    public bool NotifyEmailOnSubscribed { get; set; }
    public bool NotifyPushOnSubscribed { get; set; }
    public bool NotifyTelegramOnSubscribed { get; set; }
}
