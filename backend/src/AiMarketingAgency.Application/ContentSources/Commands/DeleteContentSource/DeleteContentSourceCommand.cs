using MediatR;

namespace AiMarketingAgency.Application.ContentSources.Commands.DeleteContentSource;

public record DeleteContentSourceCommand(Guid Id) : IRequest;
