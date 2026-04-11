using AiMarketingAgency.Application.Agencies.Dtos;
using AiMarketingAgency.Domain.Enums;
using AiMarketingAgency.Domain.ValueObjects;
using MediatR;

namespace AiMarketingAgency.Application.Agencies.Commands.CreateAgency;

public record CreateAgencyCommand : IRequest<AgencyDto>
{
    public string Name { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? WebsiteUrl { get; init; }
    public BrandVoice? BrandVoice { get; init; }
    public TargetAudience? TargetAudience { get; init; }
    public Guid? DefaultLlmProviderKeyId { get; init; }
    public ApprovalMode ApprovalMode { get; init; } = ApprovalMode.Manual;
    public int AutoApproveMinScore { get; init; } = 7;
}
