using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AiMarketingAgency.Application.Approvals.Commands.ApproveContent;

public class ApproveContentCommandHandler : IRequestHandler<ApproveContentCommand>
{
    private readonly IAppDbContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly ISocialPublishingServiceFactory _socialFactory;
    private readonly ILogger<ApproveContentCommandHandler> _logger;

    public ApproveContentCommandHandler(
        IAppDbContext context,
        ITenantContext tenantContext,
        ISocialPublishingServiceFactory socialFactory,
        ILogger<ApproveContentCommandHandler> logger)
    {
        _context = context;
        _tenantContext = tenantContext;
        _socialFactory = socialFactory;
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

        // Auto-publish to connected social platforms
        var connectors = await _context.SocialConnectors
            .Where(c => c.AgencyId == request.AgencyId && c.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var connector in connectors)
        {
            try
            {
                var publisher = _socialFactory.Create(connector.Platform);
                var result = await publisher.PublishAsync(connector, content, cancellationToken);
                if (result.Success)
                    _logger.LogInformation("Auto-published content {ContentId} to {Platform}: {PostUrl}",
                        content.Id, connector.Platform, result.PostUrl);
                else
                    _logger.LogWarning("Failed to auto-publish content {ContentId} to {Platform}: {Error}",
                        content.Id, connector.Platform, result.Error);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error auto-publishing content {ContentId} to {Platform}",
                    content.Id, connector.Platform);
            }
        }
    }
}
