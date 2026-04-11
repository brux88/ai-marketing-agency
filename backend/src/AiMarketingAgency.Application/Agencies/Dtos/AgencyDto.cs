using AiMarketingAgency.Domain.Enums;
using AiMarketingAgency.Domain.ValueObjects;

namespace AiMarketingAgency.Application.Agencies.Dtos;

public class AgencyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? LogoUrl { get; set; }
    public BrandVoice BrandVoice { get; set; } = new();
    public TargetAudience TargetAudience { get; set; } = new();
    public Guid? DefaultLlmProviderKeyId { get; set; }
    public Guid? ImageLlmProviderKeyId { get; set; }
    public ApprovalMode ApprovalMode { get; set; }
    public int AutoApproveMinScore { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ContentSourcesCount { get; set; }
    public int GeneratedContentsCount { get; set; }
}
