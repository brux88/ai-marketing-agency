using MediatR;

namespace AiMarketingAgency.Application.Approvals.Commands.RejectContent;

public record RejectContentCommand(Guid ContentId, Guid AgencyId) : IRequest;
