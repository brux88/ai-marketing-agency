using AiMarketingAgency.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Application.Agencies.Commands.UpdateTargetAudience;

public class UpdateTargetAudienceCommandHandler : IRequestHandler<UpdateTargetAudienceCommand>
{
    private readonly IAppDbContext _context;

    public UpdateTargetAudienceCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateTargetAudienceCommand request, CancellationToken cancellationToken)
    {
        var agency = await _context.Agencies
            .FirstOrDefaultAsync(a => a.Id == request.AgencyId && a.IsActive, cancellationToken)
            ?? throw new KeyNotFoundException("Agency not found.");

        agency.TargetAudience = request.TargetAudience;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
