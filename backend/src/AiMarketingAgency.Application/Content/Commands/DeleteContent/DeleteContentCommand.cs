using MediatR;

namespace AiMarketingAgency.Application.Content.Commands.DeleteContent;

public record DeleteContentCommand(Guid AgencyId, Guid ContentId) : IRequest<bool>;
