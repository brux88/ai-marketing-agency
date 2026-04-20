using AiMarketingAgency.Application.Common;
using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Application.Email;
using AiMarketingAgency.Application.Newsletter;
using AiMarketingAgency.Domain.Entities;
using AiMarketingAgency.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AiMarketingAgency.Application.Approvals.Commands.ApproveContent;

public class ApproveContentCommandHandler : IRequestHandler<ApproveContentCommand>
{
    private readonly IAppDbContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly INotificationService _notificationService;
    private readonly IEmailNotificationService _emailNotificationService;
    private readonly IPushNotificationService _pushNotificationService;
    private readonly IEmailSendingService _emailSendingService;
    private readonly ITelegramBotService _telegramBot;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ApproveContentCommandHandler> _logger;

    public ApproveContentCommandHandler(
        IAppDbContext context,
        ITenantContext tenantContext,
        INotificationService notificationService,
        IEmailNotificationService emailNotificationService,
        IPushNotificationService pushNotificationService,
        IEmailSendingService emailSendingService,
        ITelegramBotService telegramBot,
        IConfiguration configuration,
        ILogger<ApproveContentCommandHandler> logger)
    {
        _context = context;
        _tenantContext = tenantContext;
        _notificationService = notificationService;
        _emailNotificationService = emailNotificationService;
        _pushNotificationService = pushNotificationService;
        _emailSendingService = emailSendingService;
        _telegramBot = telegramBot;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task Handle(ApproveContentCommand request, CancellationToken cancellationToken)
    {
        var content = await _context.GeneratedContents
            .FirstOrDefaultAsync(c => c.Id == request.ContentId && c.AgencyId == request.AgencyId, cancellationToken)
            ?? throw new KeyNotFoundException("Content not found.");

        if (content.Status != ContentStatus.InReview && content.Status != ContentStatus.Draft && content.Status != ContentStatus.Rejected)
            throw new InvalidOperationException("Only content in review, draft, or rejected can be approved.");

        content.Status = ContentStatus.Approved;
        content.ApprovedAt = DateTime.UtcNow;
        content.ApprovedBy = _tenantContext.UserId;
        content.AutoApproved = false;

        await _context.SaveChangesAsync(cancellationToken);

        await CalendarAutoScheduler.TryScheduleAsync(_context, content, _logger, cancellationToken);

        // Auto-send newsletter on approval
        if (content.ContentType == ContentType.Newsletter)
        {
            try
            {
                EmailConnector? connector = null;
                if (content.ProjectId.HasValue)
                {
                    connector = await _context.EmailConnectors
                        .FirstOrDefaultAsync(e => e.AgencyId == content.AgencyId
                            && e.ProjectId == content.ProjectId
                            && e.IsActive, cancellationToken);
                }
                connector ??= await _context.EmailConnectors
                    .FirstOrDefaultAsync(e => e.AgencyId == content.AgencyId
                        && e.ProjectId == null
                        && e.IsActive, cancellationToken);

                if (connector != null)
                {
                    var subsQuery = _context.NewsletterSubscribers
                        .Where(s => s.AgencyId == content.AgencyId && s.IsActive);
                    if (content.ProjectId.HasValue)
                        subsQuery = subsQuery.Where(s => s.ProjectId == content.ProjectId.Value);
                    else
                        subsQuery = subsQuery.Where(s => s.ProjectId == null);
                    var subscribers = await subsQuery.ToListAsync(cancellationToken);

                    if (subscribers.Count > 0)
                    {
                        var frontendUrl = _configuration["Frontend:Url"] ?? NewsletterLinks.DefaultFrontendUrl;
                        var htmlTemplate = EmailTemplates.Newsletter(
                            content.Title, content.Body, "{{UNSUBSCRIBE_URL}}");
                        await _emailSendingService.SendNewsletterAsync(
                            connector, subscribers, content.Title, htmlTemplate,
                            s => NewsletterLinks.UnsubscribeUrl(frontendUrl, s),
                            cancellationToken);
                        _logger.LogInformation("Auto-sent newsletter '{Title}' to {Count} subscribers",
                            content.Title, subscribers.Count);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to auto-send newsletter on approval for content {ContentId}", content.Id);
            }
        }

        try
        {
            await _notificationService.NotifyContentApproved(
                content.TenantId, content.AgencyId, content.Id, content.Title);
            await _telegramBot.NotifyAgencyAsync(content.AgencyId, content.ProjectId,
                $"\u2705 <b>Contenuto approvato</b>\n{content.Title}", cancellationToken);

            // Email notification on approval
            if (content.ProjectId.HasValue)
            {
                var project = await _context.Projects.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == content.ProjectId.Value, cancellationToken);

                if (project?.NotifyEmailOnApprovalNeeded == true && !string.IsNullOrWhiteSpace(project.NotificationEmail))
                {
                    var subject = $"Contenuto approvato - {content.Title}";
                    var htmlBody = $"""
                        <h2>Contenuto approvato</h2>
                        <p>Il contenuto <strong>{content.Title}</strong> del progetto <strong>{project.Name}</strong> e stato approvato.</p>
                        <p>Tipo: {content.ContentType}</p>
                        """;
                    await _emailNotificationService.SendEmailNotificationAsync(
                        content.AgencyId, content.ProjectId, subject, htmlBody, cancellationToken);
                }

                await _pushNotificationService.SendToProjectAsync(
                    content.AgencyId,
                    content.ProjectId,
                    PushEventType.ApprovalNeeded,
                    "Contenuto approvato",
                    content.Title,
                    new Dictionary<string, string>
                    {
                        ["agencyId"] = content.AgencyId.ToString(),
                        ["projectId"] = content.ProjectId.Value.ToString(),
                        ["contentId"] = content.Id.ToString(),
                        ["event"] = "content.approved",
                    },
                    cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send approve notification for content {ContentId}", content.Id);
        }
    }
}
