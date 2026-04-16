using AiMarketingAgency.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Application.Agencies.Commands.UpdateApprovalMode;

public class UpdateApprovalModeCommandHandler : IRequestHandler<UpdateApprovalModeCommand>
{
    private readonly IAppDbContext _context;

    public UpdateApprovalModeCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateApprovalModeCommand request, CancellationToken cancellationToken)
    {
        var agency = await _context.Agencies
            .FirstOrDefaultAsync(a => a.Id == request.AgencyId && a.IsActive, cancellationToken)
            ?? throw new KeyNotFoundException("Agency not found.");

        agency.ApprovalMode = request.ApprovalMode;
        agency.AutoApproveMinScore = request.AutoApproveMinScore;
        agency.AutoScheduleOnApproval = request.AutoScheduleOnApproval;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
