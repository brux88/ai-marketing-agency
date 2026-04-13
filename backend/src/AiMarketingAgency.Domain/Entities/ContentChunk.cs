using AiMarketingAgency.Domain.Interfaces;

namespace AiMarketingAgency.Domain.Entities;

public class ContentChunk : BaseEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid AgencyId { get; set; }
    public Guid ContentSourceId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public float[] Embedding { get; set; } = Array.Empty<float>();
    public DateTime FetchedAt { get; set; }
    public int ChunkIndex { get; set; }

    // Navigation
    public ContentSource ContentSource { get; set; } = null!;
}
