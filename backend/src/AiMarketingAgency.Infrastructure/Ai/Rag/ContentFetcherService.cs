using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Domain.Entities;
using AiMarketingAgency.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AiMarketingAgency.Infrastructure.Ai.Rag;

public class ContentFetcherService : IContentFetcherService
{
    private readonly IAppDbContext _context;
    private readonly RssFeedFetcher _rssFetcher;
    private readonly WebScraperFetcher _webScraper;
    private readonly EmbeddingService _embeddingService;
    private readonly ILlmKeyVault _keyVault;
    private readonly ILogger<ContentFetcherService> _logger;

    public ContentFetcherService(
        IAppDbContext context,
        RssFeedFetcher rssFetcher,
        WebScraperFetcher webScraper,
        EmbeddingService embeddingService,
        ILlmKeyVault keyVault,
        ILogger<ContentFetcherService> logger)
    {
        _context = context;
        _rssFetcher = rssFetcher;
        _webScraper = webScraper;
        _embeddingService = embeddingService;
        _keyVault = keyVault;
        _logger = logger;
    }

    public async Task FetchAndIndexSourceAsync(ContentSource source, Guid tenantId, Guid agencyId, CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching content from source {SourceId} ({Type}): {Url}", source.Id, source.Type, source.Url);

        var articles = source.Type switch
        {
            ContentSourceType.RssFeed => await _rssFetcher.FetchAsync(source.Url, ct),
            ContentSourceType.Website => await _webScraper.FetchAsync(source.Url, ct),
            _ => new List<FetchedArticle>()
        };

        if (!articles.Any()) return;

        // Get an OpenAI key for embeddings
        var openAiKey = await GetEmbeddingApiKeyAsync(agencyId, ct);
        if (string.IsNullOrEmpty(openAiKey))
        {
            _logger.LogWarning("No OpenAI key found for embeddings, skipping indexing for agency {AgencyId}", agencyId);
            return;
        }

        // Remove old chunks for this source
        var oldChunks = await _context.ContentChunks
            .Where(c => c.ContentSourceId == source.Id)
            .ToListAsync(ct);

        foreach (var old in oldChunks)
            _context.ContentChunks.Remove(old);

        // Chunk and embed
        foreach (var article in articles)
        {
            var fullText = $"{article.Title}\n\n{article.Content}";
            var chunks = _embeddingService.ChunkText(fullText);

            for (int i = 0; i < chunks.Count; i++)
            {
                try
                {
                    var embedding = await _embeddingService.GenerateEmbeddingAsync(chunks[i], openAiKey, ct);

                    _context.ContentChunks.Add(new ContentChunk
                    {
                        TenantId = tenantId,
                        AgencyId = agencyId,
                        ContentSourceId = source.Id,
                        Title = article.Title,
                        Content = chunks[i],
                        Url = article.Url,
                        Embedding = embedding,
                        FetchedAt = DateTime.UtcNow,
                        ChunkIndex = i
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to embed chunk {Index} for article '{Title}'", i, article.Title);
                }
            }
        }

        source.LastFetchedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Indexed {Count} articles from source {SourceId}", articles.Count, source.Id);
    }

    public async Task<List<ContentChunk>> SearchRelevantChunksAsync(string query, Guid agencyId, int topK = 5, CancellationToken ct = default)
    {
        var openAiKey = await GetEmbeddingApiKeyAsync(agencyId, ct);
        if (string.IsNullOrEmpty(openAiKey))
            return new List<ContentChunk>();

        try
        {
            var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(query, openAiKey, ct);

            // Simple cosine similarity in-memory (for production, use pgvector)
            var allChunks = await _context.ContentChunks
                .Where(c => c.AgencyId == agencyId)
                .ToListAsync(ct);

            return allChunks
                .Where(c => c.Embedding.Length > 0)
                .Select(c => new { Chunk = c, Similarity = CosineSimilarity(queryEmbedding, c.Embedding) })
                .OrderByDescending(x => x.Similarity)
                .Take(topK)
                .Select(x => x.Chunk)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to search relevant chunks for agency {AgencyId}", agencyId);
            return new List<ContentChunk>();
        }
    }

    public async Task RefreshAllSourcesAsync(Guid agencyId, CancellationToken ct = default)
    {
        var sources = await _context.ContentSources
            .Where(s => s.AgencyId == agencyId && s.IsActive)
            .ToListAsync(ct);

        var agency = await _context.Agencies.FirstOrDefaultAsync(a => a.Id == agencyId, ct);
        if (agency == null) return;

        foreach (var source in sources)
        {
            await FetchAndIndexSourceAsync(source, agency.TenantId, agencyId, ct);
        }
    }

    private async Task<string?> GetEmbeddingApiKeyAsync(Guid agencyId, CancellationToken ct)
    {
        // Try to find an OpenAI key for the agency's tenant
        var agency = await _context.Agencies
            .Include(a => a.DefaultLlmProviderKey)
            .FirstOrDefaultAsync(a => a.Id == agencyId, ct);

        if (agency?.DefaultLlmProviderKey?.ProviderType == LlmProviderType.OpenAI)
            return agency.DefaultLlmProviderKey.EncryptedApiKey;

        // Fallback: find any OpenAI key in the tenant
        var key = await _context.LlmProviderKeys
            .FirstOrDefaultAsync(k => k.ProviderType == LlmProviderType.OpenAI && k.IsActive, ct);

        return key?.EncryptedApiKey;
    }

    private static double CosineSimilarity(float[] a, float[] b)
    {
        if (a.Length != b.Length) return 0;
        double dot = 0, magA = 0, magB = 0;
        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            magA += a[i] * a[i];
            magB += b[i] * b[i];
        }
        var denom = Math.Sqrt(magA) * Math.Sqrt(magB);
        return denom == 0 ? 0 : dot / denom;
    }
}
