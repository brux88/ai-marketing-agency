using AiMarketingAgency.Domain.Enums;
using MediatR;

namespace AiMarketingAgency.Application.Agencies.Commands.UpdateApprovalMode;

public record UpdateApprovalModeCommand(Guid AgencyId, ApprovalMode ApprovalMode, int AutoApproveMinScore) : IRequest;
