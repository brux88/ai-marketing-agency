using AiMarketingAgency.Domain.ValueObjects;
using MediatR;

namespace AiMarketingAgency.Application.Agencies.Commands.UpdateTargetAudience;

public record UpdateTargetAudienceCommand(Guid AgencyId, TargetAudience TargetAudience) : IRequest;
