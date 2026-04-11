using AiMarketingAgency.Domain.Enums;

namespace AiMarketingAgency.Application.ContentSources.Dtos;

public class ContentSourceDto
{
    public Guid Id { get; set; }
    public Guid AgencyId { get; set; }
    public Guid? ProjectId { get; set; }
    public ContentSourceType Type { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Config { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastFetchedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
