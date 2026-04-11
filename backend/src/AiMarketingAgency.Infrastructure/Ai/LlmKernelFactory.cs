using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace AiMarketingAgency.Infrastructure.Ai;

public class LlmKernelFactory : ILlmKernelFactory
{
    private readonly IAppDbContext _context;
    private readonly ILlmKeyVault _keyVault;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<LlmKernelFactory> _logger;

    public LlmKernelFactory(
        IAppDbContext context,
        ILlmKeyVault keyVault,
        IHttpClientFactory httpClientFactory,
        ILogger<LlmKernelFactory> logger)
    {
        _context = context;
        _keyVault = keyVault;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<Kernel> CreateKernelAsync(Guid agencyId, CancellationToken ct = default)
    {
        var agency = await _context.Agencies
            .Include(a => a.DefaultLlmProviderKey)
            .FirstOrDefaultAsync(a => a.Id == agencyId, ct)
            ?? throw new InvalidOperationException($"Agency {agencyId} not found.");

        var providerKey = agency.DefaultLlmProviderKey
            ?? throw new InvalidOperationException($"Agency {agencyId} has no default LLM provider key configured.");

        if (!providerKey.IsActive)
            throw new InvalidOperationException($"LLM provider key '{providerKey.DisplayName}' is not active.");

        // Use the key directly from the loaded entity (stored as plain text in dev)
        var apiKey = providerKey.EncryptedApiKey;

        var builder = Kernel.CreateBuilder();
        var modelName = providerKey.ModelName ?? GetDefaultModelName(providerKey.ProviderType);

        switch (providerKey.ProviderType)
        {
            case LlmProviderType.OpenAI:
                builder.AddOpenAIChatCompletion(
                    modelId: modelName,
                    apiKey: apiKey);
                break;

            case LlmProviderType.AzureOpenAI:
                var endpoint = providerKey.BaseUrl
                    ?? throw new InvalidOperationException("Azure OpenAI requires a base URL (endpoint).");
                builder.AddAzureOpenAIChatCompletion(
                    deploymentName: modelName,
                    endpoint: endpoint,
                    apiKey: apiKey);
                break;

            case LlmProviderType.Anthropic:
            case LlmProviderType.NanoBanana:
            case LlmProviderType.HiggField:
            case LlmProviderType.Custom:
                // Use OpenAI-compatible HTTP client for non-natively-supported providers
                var baseUrl = providerKey.BaseUrl ?? GetDefaultBaseUrl(providerKey.ProviderType);
                var httpClient = _httpClientFactory.CreateClient($"LlmProvider_{providerKey.Id}");
                httpClient.BaseAddress = new Uri(baseUrl);

                builder.AddOpenAIChatCompletion(
                    modelId: modelName,
                    apiKey: apiKey,
                    httpClient: httpClient);
                break;

            default:
                throw new InvalidOperationException($"Unsupported LLM provider type: {providerKey.ProviderType}");
        }

        _logger.LogInformation(
            "Created Semantic Kernel for agency {AgencyId} with provider {Provider}, model {Model}",
            agencyId, providerKey.ProviderType, modelName);

        return builder.Build();
    }

    private static string GetDefaultModelName(LlmProviderType providerType) => providerType switch
    {
        LlmProviderType.OpenAI => "gpt-4o",
        LlmProviderType.Anthropic => "claude-sonnet-4-20250514",
        LlmProviderType.AzureOpenAI => "gpt-4o",
        LlmProviderType.NanoBanana => "nanobanana-v1",
        LlmProviderType.HiggField => "higgfield-v1",
        LlmProviderType.Custom => "default",
        _ => "gpt-4o"
    };

    private static string GetDefaultBaseUrl(LlmProviderType providerType) => providerType switch
    {
        LlmProviderType.Anthropic => "https://api.anthropic.com/v1/",
        LlmProviderType.NanoBanana => "https://api.nanobanana.com/v1/",
        LlmProviderType.HiggField => "https://api.higgfield.ai/v1/",
        LlmProviderType.Custom => "http://localhost:8080/v1/",
        _ => "https://api.openai.com/v1/"
    };
}
