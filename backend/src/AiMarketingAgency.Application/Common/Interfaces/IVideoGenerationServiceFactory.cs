using AiMarketingAgency.Domain.Enums;

namespace AiMarketingAgency.Application.Common.Interfaces;

public interface IVideoGenerationServiceFactory
{
    IVideoGenerationService Create(LlmProviderType providerType, string apiKey, string? apiKeySecret = null, string? baseUrl = null);
}
