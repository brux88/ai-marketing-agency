using AiMarketingAgency.Application.LlmKeys.Dtos;
using AiMarketingAgency.Domain.Enums;
using MediatR;

namespace AiMarketingAgency.Application.LlmKeys.Commands.AddLlmKey;

public record AddLlmKeyCommand : IRequest<LlmKeyDto>
{
    public LlmProviderType ProviderType { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string ApiKey { get; init; } = string.Empty;
    public string? ApiKeySecret { get; init; }
    public string? ModelName { get; init; }
    public string? BaseUrl { get; init; }
    public LlmProviderCategory Category { get; init; } = LlmProviderCategory.Text;
}
