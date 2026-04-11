using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Domain.Enums;
using AiMarketingAgency.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Infrastructure.Security;

/// <summary>
/// Simple DB-based key vault. In production, swap with Azure Key Vault.
/// Keys are stored in LlmProviderKeys table (EncryptedApiKey column).
/// For dev, keys are stored as-is; in production, use ASP.NET Data Protection to encrypt.
/// </summary>
public class LlmKeyVault : ILlmKeyVault
{
    private readonly AppDbContext _db;

    public LlmKeyVault(AppDbContext db)
    {
        _db = db;
    }

    public async Task StoreKeyAsync(Guid tenantId, LlmProviderType provider, string plainTextKey, CancellationToken ct = default)
    {
        var existing = await _db.LlmProviderKeys
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(k => k.TenantId == tenantId && k.ProviderType == provider, ct);

        if (existing != null)
        {
            existing.EncryptedApiKey = plainTextKey;
        }
        else
        {
            _db.LlmProviderKeys.Add(new Domain.Entities.LlmProviderKey
            {
                TenantId = tenantId,
                ProviderType = provider,
                EncryptedApiKey = plainTextKey,
                DisplayName = provider.ToString(),
                IsActive = true
            });
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task<string> GetKeyAsync(Guid tenantId, LlmProviderType provider, CancellationToken ct = default)
    {
        var key = await _db.LlmProviderKeys
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(k => k.TenantId == tenantId && k.ProviderType == provider && k.IsActive, ct);

        return key?.EncryptedApiKey ?? throw new InvalidOperationException($"No API key found for provider {provider}");
    }

    public async Task DeleteKeyAsync(Guid tenantId, LlmProviderType provider, CancellationToken ct = default)
    {
        var key = await _db.LlmProviderKeys
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(k => k.TenantId == tenantId && k.ProviderType == provider, ct);

        if (key != null)
        {
            _db.LlmProviderKeys.Remove(key);
            await _db.SaveChangesAsync(ct);
        }
    }
}
