using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace AiMarketingAgency.Infrastructure.Ai.Rag;

public class EmbeddingService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<EmbeddingService> _logger;

    public EmbeddingService(IHttpClientFactory httpClientFactory, ILogger<EmbeddingService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text, string apiKey, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

        var request = new
        {
            model = "text-embedding-3-small",
            input = text[..Math.Min(text.Length, 8000)]
        };

        var response = await client.PostAsJsonAsync("https://api.openai.com/v1/embeddings", request, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonDocument>(ct);
        var embeddingArray = json!.RootElement
            .GetProperty("data")[0]
            .GetProperty("embedding");

        var embedding = new float[embeddingArray.GetArrayLength()];
        int i = 0;
        foreach (var val in embeddingArray.EnumerateArray())
        {
            embedding[i++] = val.GetSingle();
        }

        return embedding;
    }

    public List<string> ChunkText(string text, int chunkSize = 500, int overlap = 50)
    {
        var chunks = new List<string>();
        if (string.IsNullOrEmpty(text)) return chunks;

        for (int i = 0; i < text.Length; i += chunkSize - overlap)
        {
            var end = Math.Min(i + chunkSize, text.Length);
            chunks.Add(text[i..end]);
            if (end == text.Length) break;
        }

        return chunks;
    }
}
