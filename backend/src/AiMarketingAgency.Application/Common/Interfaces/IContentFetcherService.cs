using AiMarketingAgency.Domain.Entities;

namespace AiMarketingAgency.Application.Common.Interfaces;

public interface IContentFetcherService
{
    Task FetchAndIndexSourceAsync(ContentSource source, Guid tenantId, Guid agencyId, CancellationToken ct = default);
    Task<List<ContentChunk>> SearchRelevantChunksAsync(string query, Guid agencyId, int topK = 5, CancellationToken ct = default);
    Task RefreshAllSourcesAsync(Guid agencyId, CancellationToken ct = default);
}
