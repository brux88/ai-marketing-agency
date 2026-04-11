using AiMarketingAgency.Domain.Enums;
using AiMarketingAgency.Domain.Interfaces;

namespace AiMarketingAgency.Domain.Entities;

public class GeneratedContent : BaseEntity, ITenantScoped
{
    public Guid AgencyId { get; set; }
    public Guid TenantId { get; set; }
    public Guid? JobId { get; set; }
    public ContentType ContentType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? Metadata { get; set; }
    public ContentStatus Status { get; set; } = ContentStatus.Draft;
    public decimal QualityScore { get; set; }
    public decimal RelevanceScore { get; set; }
    public decimal SeoScore { get; set; }
    public decimal BrandVoiceScore { get; set; }
    public decimal OverallScore { get; set; }
    public string? ScoreExplanation { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    public bool AutoApproved { get; set; }
    public string? ImageUrl { get; set; }
    public string? ImagePrompt { get; set; }
    public string? ImageUrls { get; set; } // JSON array for carousel
    public string? VideoUrl { get; set; }
    public string? VideoPrompt { get; set; }
    public int? VideoDurationSeconds { get; set; }
    public Guid? ProjectId { get; set; }

    // Navigation
    public Agency Agency { get; set; } = null!;
    public AgentJob? Job { get; set; }
    public Project? Project { get; set; }
    public ICollection<EditorialCalendarEntry> CalendarEntries { get; set; } = new List<EditorialCalendarEntry>();
}
