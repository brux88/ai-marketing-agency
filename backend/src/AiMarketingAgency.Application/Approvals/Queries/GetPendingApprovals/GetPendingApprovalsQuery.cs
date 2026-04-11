using AiMarketingAgency.Application.Approvals.Dtos;
using MediatR;

namespace AiMarketingAgency.Application.Approvals.Queries.GetPendingApprovals;

public record GetPendingApprovalsQuery(Guid AgencyId) : IRequest<List<PendingApprovalDto>>;
