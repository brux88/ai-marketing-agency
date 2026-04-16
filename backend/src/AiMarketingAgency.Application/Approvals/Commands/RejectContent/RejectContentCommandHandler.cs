using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AiMarketingAgency.Application.Approvals.Commands.RejectContent;

public class RejectContentCommandHandler : IRequestHandler<RejectContentCommand>
{
    private readonly IAppDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly ILogger<RejectContentCommandHandler> _logger;

    public RejectContentCommandHandler(
        IAppDbContext context,
        INotificationService notificationService,
        ILogger<RejectContentCommandHandler> logger)
    {
        _context = context;
        _notificationService = notificationService;
        _logger = logger;
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

        try
        {
            await _notificationService.NotifyContentRejected(
                content.TenantId, content.AgencyId, content.Id, content.Title);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send reject notification for content {ContentId}", content.Id);
        }
    }
}
