using AiMarketingAgency.Domain.Enums;

namespace AiMarketingAgency.Application.LlmKeys.Dtos;

public class LlmKeyDto
{
    public Guid Id { get; set; }
    public LlmProviderType ProviderType { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string MaskedKey { get; set; } = string.Empty;
    public string? ModelName { get; set; }
    public string? BaseUrl { get; set; }
    public LlmProviderCategory Category { get; set; }
    public bool HasApiKeySecret { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
