using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Application.LlmKeys.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Application.LlmKeys.Queries.GetLlmKeys;

public class GetLlmKeysQueryHandler : IRequestHandler<GetLlmKeysQuery, List<LlmKeyDto>>
{
    private readonly IAppDbContext _context;

    public GetLlmKeysQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<LlmKeyDto>> Handle(GetLlmKeysQuery request, CancellationToken cancellationToken)
    {
        return await _context.LlmProviderKeys
            .Where(k => k.IsActive)
            .Select(k => new LlmKeyDto
            {
                Id = k.Id,
                ProviderType = k.ProviderType,
                DisplayName = k.DisplayName,
                MaskedKey = k.EncryptedApiKey.Length > 8
                    ? k.EncryptedApiKey.Substring(0, 4) + "...****"
                    : "****",
                ModelName = k.ModelName,
                BaseUrl = k.BaseUrl,
                Category = k.Category,
                HasApiKeySecret = k.EncryptedApiKeySecret != null && k.EncryptedApiKeySecret != "",
                IsActive = k.IsActive,
                CreatedAt = k.CreatedAt
            })
            .OrderByDescending(k => k.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
