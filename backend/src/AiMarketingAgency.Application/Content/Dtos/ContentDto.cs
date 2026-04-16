using AiMarketingAgency.Domain.Enums;

namespace AiMarketingAgency.Application.Content.Dtos;

public class ContentDto
{
    public Guid Id { get; set; }
    public Guid AgencyId { get; set; }
    public ContentType ContentType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public ContentStatus Status { get; set; }
    public decimal QualityScore { get; set; }
    public decimal RelevanceScore { get; set; }
    public decimal SeoScore { get; set; }
    public decimal BrandVoiceScore { get; set; }
    public decimal OverallScore { get; set; }
    public string? ScoreExplanation { get; set; }
    public bool AutoApproved { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public decimal? AiGenerationCostUsd { get; set; }
    public decimal? AiImageCostUsd { get; set; }
    public string? ImageUrl { get; set; }
    public string? ImagePrompt { get; set; }
    public List<string>? ImageUrls { get; set; }
    public string? VideoUrl { get; set; }
    public Guid? ProjectId { get; set; }
    public bool IsScheduled { get; set; }
}
