using AiMarketingAgency.Domain.ValueObjects;
using MediatR;

namespace AiMarketingAgency.Application.Agencies.Commands.UpdateBrandVoice;

public record UpdateBrandVoiceCommand(Guid AgencyId, BrandVoice BrandVoice) : IRequest;
