using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Application.Content.Commands.DeleteContent;

public class DeleteContentCommandHandler : IRequestHandler<DeleteContentCommand, bool>
{
    private readonly IAppDbContext _context;

    public DeleteContentCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteContentCommand request, CancellationToken cancellationToken)
    {
        var content = await _context.GeneratedContents
            .FirstOrDefaultAsync(
                c => c.Id == request.ContentId && c.AgencyId == request.AgencyId,
                cancellationToken);

        if (content == null) return false;

        content.IsDeleted = true;
        content.DeletedAt = DateTime.UtcNow;

        var pendingEntries = await _context.CalendarEntries
            .Where(e => e.ContentId == request.ContentId
                        && (e.Status == CalendarEntryStatus.Scheduled || e.Status == CalendarEntryStatus.Draft))
            .ToListAsync(cancellationToken);
        _context.CalendarEntries.RemoveRange(pendingEntries);

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
