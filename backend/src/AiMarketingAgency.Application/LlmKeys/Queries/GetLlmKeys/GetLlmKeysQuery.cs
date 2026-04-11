using AiMarketingAgency.Application.LlmKeys.Dtos;
using MediatR;

namespace AiMarketingAgency.Application.LlmKeys.Queries.GetLlmKeys;

public record GetLlmKeysQuery : IRequest<List<LlmKeyDto>>;
