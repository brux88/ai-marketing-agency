using AiMarketingAgency.Application.Content.Dtos;
using MediatR;

namespace AiMarketingAgency.Application.Content.Commands.UpdateContent;

public record UpdateContentCommand(Guid AgencyId, Guid ContentId, string Title, string Body) : IRequest<ContentDto>;
