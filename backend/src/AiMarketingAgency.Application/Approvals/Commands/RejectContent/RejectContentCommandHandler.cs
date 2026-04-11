using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Application.Approvals.Commands.RejectContent;

public class RejectContentCommandHandler : IRequestHandler<RejectContentCommand>
{
    private readonly IAppDbContext _context;

    public RejectContentCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(RejectContentCommand request, CancellationToken cancellationToken)
    {
        var content = await _context.GeneratedContents
            .FirstOrDefaultAsync(c => c.Id == request.ContentId && c.AgencyId == request.AgencyId, cancellationToken)
            ?? throw new KeyNotFoundException("Content not found.");

        if (content.Status != ContentStatus.InReview && content.Status != ContentStatus.Draft)
            throw new InvalidOperationException("Only content in review or draft can be rejected.");

        content.Status = ContentStatus.Rejected;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
