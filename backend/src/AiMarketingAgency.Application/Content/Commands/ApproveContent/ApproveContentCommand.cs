using MediatR;

namespace AiMarketingAgency.Application.Content.Commands.ApproveContent;

public record ApproveContentCommand(Guid ContentId) : IRequest<bool>;
