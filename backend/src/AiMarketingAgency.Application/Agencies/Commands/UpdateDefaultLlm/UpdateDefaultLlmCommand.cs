using MediatR;

namespace AiMarketingAgency.Application.Agencies.Commands.UpdateDefaultLlm;

public record UpdateDefaultLlmCommand(Guid AgencyId, Guid? DefaultLlmProviderKeyId, Guid? ImageLlmProviderKeyId) : IRequest;
