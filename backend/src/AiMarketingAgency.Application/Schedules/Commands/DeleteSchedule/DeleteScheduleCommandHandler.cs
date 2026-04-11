using AiMarketingAgency.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Application.Schedules.Commands.DeleteSchedule;

public class DeleteScheduleCommandHandler : IRequestHandler<DeleteScheduleCommand>
{
    private readonly IAppDbContext _context;

    public DeleteScheduleCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteScheduleCommand request, CancellationToken cancellationToken)
    {
        var schedule = await _context.ContentSchedules
            .FirstOrDefaultAsync(s => s.Id == request.Id && s.AgencyId == request.AgencyId, cancellationToken)
            ?? throw new KeyNotFoundException("Schedule not found.");

        _context.ContentSchedules.Remove(schedule);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
