using MediatR;

namespace AiMarketingAgency.Application.Approvals.Commands.ApproveContent;

public record ApproveContentCommand(Guid ContentId, Guid AgencyId) : IRequest;
