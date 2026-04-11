using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Application.Content.Commands.ApproveContent;

public class ApproveContentCommandHandler : IRequestHandler<ApproveContentCommand, bool>
{
    private readonly IAppDbContext _context;
    private readonly ITenantContext _tenantContext;

    public ApproveContentCommandHandler(IAppDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<bool> Handle(ApproveContentCommand request, CancellationToken cancellationToken)
    {
        var content = await _context.GeneratedContents
            .FirstOrDefaultAsync(c => c.Id == request.ContentId, cancellationToken);

        if (content == null) return false;

        content.Status = ContentStatus.Approved;
        content.ApprovedAt = DateTime.UtcNow;
        content.ApprovedBy = _tenantContext.UserId;
        content.AutoApproved = false;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
