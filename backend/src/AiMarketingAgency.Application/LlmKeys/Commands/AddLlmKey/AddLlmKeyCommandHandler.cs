using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Application.LlmKeys.Dtos;
using AiMarketingAgency.Domain.Entities;
using MediatR;

namespace AiMarketingAgency.Application.LlmKeys.Commands.AddLlmKey;

public class AddLlmKeyCommandHandler : IRequestHandler<AddLlmKeyCommand, LlmKeyDto>
{
    private readonly IAppDbContext _context;
    private readonly ITenantContext _tenantContext;

    public AddLlmKeyCommandHandler(IAppDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<LlmKeyDto> Handle(AddLlmKeyCommand request, CancellationToken cancellationToken)
    {
        var key = new LlmProviderKey
        {
            TenantId = _tenantContext.TenantId,
            ProviderType = request.ProviderType,
            DisplayName = request.DisplayName,
            EncryptedApiKey = request.ApiKey, // Stored as-is for dev; use Data Protection or Key Vault in production
            EncryptedApiKeySecret = request.ApiKeySecret,
            ModelName = request.ModelName,
            BaseUrl = request.BaseUrl,
            Category = request.Category
        };

        _context.LlmProviderKeys.Add(key);
        await _context.SaveChangesAsync(cancellationToken);

        return new LlmKeyDto
        {
            Id = key.Id,
            ProviderType = key.ProviderType,
            DisplayName = key.DisplayName,
            MaskedKey = MaskKey(request.ApiKey),
            ModelName = key.ModelName,
            BaseUrl = key.BaseUrl,
            Category = key.Category,
            HasApiKeySecret = !string.IsNullOrEmpty(key.EncryptedApiKeySecret),
            IsActive = key.IsActive,
            CreatedAt = key.CreatedAt
        };
    }

    private static string MaskKey(string key)
    {
        if (key.Length <= 8) return "****";
        return key[..4] + "..." + key[^4..];
    }
}
