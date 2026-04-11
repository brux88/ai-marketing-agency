using AiMarketingAgency.Domain.Enums;

namespace AiMarketingAgency.Application.Common.Interfaces;

public interface IImageGenerationServiceFactory
{
    IImageGenerationService Create(LlmProviderType providerType, string apiKey, string? apiKeySecret = null, string? baseUrl = null);
}
