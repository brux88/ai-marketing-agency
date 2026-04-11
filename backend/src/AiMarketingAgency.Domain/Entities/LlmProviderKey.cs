using AiMarketingAgency.Domain.Enums;
using AiMarketingAgency.Domain.Interfaces;

namespace AiMarketingAgency.Domain.Entities;

public class LlmProviderKey : BaseEntity, ITenantScoped, ISoftDeletable
{
    public Guid TenantId { get; set; }
    public LlmProviderType ProviderType { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string EncryptedApiKey { get; set; } = string.Empty;
    public string? KeyVaultSecretName { get; set; }
    public string? ModelName { get; set; }
    public string? EncryptedApiKeySecret { get; set; }
    public string? BaseUrl { get; set; }
    public LlmProviderCategory Category { get; set; } = LlmProviderCategory.Text;
    public bool IsActive { get; set; } = true;

    // Navigation
    public Tenant Tenant { get; set; } = null!;
}
