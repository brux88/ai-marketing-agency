using AiMarketingAgency.Application.Agencies.Dtos;
using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Domain.Entities;
using MediatR;

namespace AiMarketingAgency.Application.Agencies.Commands.CreateAgency;

public class CreateAgencyCommandHandler : IRequestHandler<CreateAgencyCommand, AgencyDto>
{
    private readonly IAppDbContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly IUsageGuard _usageGuard;

    public CreateAgencyCommandHandler(IAppDbContext context, ITenantContext tenantContext, IUsageGuard usageGuard)
    {
        _context = context;
        _tenantContext = tenantContext;
        _usageGuard = usageGuard;
    }

    public async Task<AgencyDto> Handle(CreateAgencyCommand request, CancellationToken cancellationToken)
    {
        if (!await _usageGuard.CanCreateAgencyAsync(_tenantContext.TenantId, cancellationToken))
            throw new InvalidOperationException("You have reached the maximum number of agencies for your plan.");

        var agency = new Agency
        {
            TenantId = _tenantContext.TenantId,
            Name = request.Name,
            ProductName = request.ProductName,
            Description = request.Description,
            WebsiteUrl = request.WebsiteUrl,
            BrandVoice = request.BrandVoice ?? new(),
            TargetAudience = request.TargetAudience ?? new(),
            DefaultLlmProviderKeyId = request.DefaultLlmProviderKeyId,
            ApprovalMode = request.ApprovalMode,
            AutoApproveMinScore = request.AutoApproveMinScore
        };

        _context.Agencies.Add(agency);
        await _context.SaveChangesAsync(cancellationToken);

        return new AgencyDto
        {
            Id = agency.Id,
            Name = agency.Name,
            ProductName = agency.ProductName,
            Description = agency.Description,
            WebsiteUrl = agency.WebsiteUrl,
            BrandVoice = agency.BrandVoice,
            TargetAudience = agency.TargetAudience,
            DefaultLlmProviderKeyId = agency.DefaultLlmProviderKeyId,
            ApprovalMode = agency.ApprovalMode,
            AutoApproveMinScore = agency.AutoApproveMinScore,
            IsActive = agency.IsActive,
            CreatedAt = agency.CreatedAt
        };
    }
}
