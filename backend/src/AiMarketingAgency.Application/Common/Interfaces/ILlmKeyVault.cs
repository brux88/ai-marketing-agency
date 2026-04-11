using AiMarketingAgency.Domain.Enums;

namespace AiMarketingAgency.Application.Common.Interfaces;

public interface ILlmKeyVault
{
    Task StoreKeyAsync(Guid tenantId, LlmProviderType provider, string plainTextKey, CancellationToken ct = default);
    Task<string> GetKeyAsync(Guid tenantId, LlmProviderType provider, CancellationToken ct = default);
    Task DeleteKeyAsync(Guid tenantId, LlmProviderType provider, CancellationToken ct = default);
}
