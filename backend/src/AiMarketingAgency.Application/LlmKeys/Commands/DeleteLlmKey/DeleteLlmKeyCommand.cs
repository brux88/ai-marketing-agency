using MediatR;

namespace AiMarketingAgency.Application.LlmKeys.Commands.DeleteLlmKey;

public record DeleteLlmKeyCommand(Guid Id) : IRequest;
