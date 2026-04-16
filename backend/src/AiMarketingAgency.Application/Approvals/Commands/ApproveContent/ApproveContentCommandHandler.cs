using AiMarketingAgency.Application.Common;
using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Domain.Entities;
using AiMarketingAgency.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AiMarketingAgency.Application.Approvals.Commands.ApproveContent;

public class ApproveContentCommandHandler : IRequestHandler<ApproveContentCommand>
{
    private readonly IAppDbContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly INotificationService _notificationService;
    private readonly ILogger<ApproveContentCommandHandler> _logger;

    public ApproveContentCommandHandler(
        IAppDbContext context,
        ITenantContext tenantContext,
        INotificationService notificationService,
        ILogger<ApproveContentCommandHandler> logger)
    {
        _context = context;
        _tenantContext = tenantContext;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Handle(ApproveContentCommand request, CancellationToken cancellationToken)
    {
        var content = await _context.GeneratedContents
            .FirstOrDefaultAsync(c => c.Id == request.ContentId && c.AgencyId == request.AgencyId, cancellationToken)
            ?? throw new KeyNotFoundException("Content not found.");

        if (content.Status != ContentStatus.InReview && content.Status != ContentStatus.Draft)
            throw new InvalidOperationException("Only content in review or draft can be approved.");

        content.Status = ContentStatus.Approved;
        content.ApprovedAt = DateTime.UtcNow;
        content.ApprovedBy = _tenantContext.UserId;
        content.AutoApproved = false;

        await _context.SaveChangesAsync(cancellationToken);

        await CalendarAutoScheduler.TryScheduleAsync(_context, content, _logger, cancellationToken);

        try
        {
            await _notificationService.NotifyContentApproved(
                content.TenantId, content.AgencyId, content.Id, content.Title);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send approve notification for content {ContentId}", content.Id);
        }
    }
}
