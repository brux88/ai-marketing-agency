using AiMarketingAgency.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AiMarketingAgency.Infrastructure.Notifications;

public class EmailNotificationService : IEmailNotificationService
{
    private readonly IAppDbContext _context;
    private readonly ITransactionalEmailService _emailService;
    private readonly ILogger<EmailNotificationService> _logger;

    public EmailNotificationService(
        IAppDbContext context,
        ITransactionalEmailService emailService,
        ILogger<EmailNotificationService> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task SendEmailNotificationAsync(Guid agencyId, Guid? projectId, string subject, string htmlBody, CancellationToken ct = default)
    {
        try
        {
            string? notificationEmail = null;

            if (projectId.HasValue)
            {
                notificationEmail = await _context.Projects
                    .AsNoTracking()
                    .Where(p => p.Id == projectId.Value && p.AgencyId == agencyId)
                    .Select(p => p.NotificationEmail)
                    .FirstOrDefaultAsync(ct);
            }

            // Fall back to agency-level notification email (also used by
            // agency-scoped events like new agency-newsletter subscribers).
            if (string.IsNullOrWhiteSpace(notificationEmail))
            {
                notificationEmail = await _context.Agencies
                    .AsNoTracking()
                    .Where(a => a.Id == agencyId)
                    .Select(a => a.NotificationEmail)
                    .FirstOrDefaultAsync(ct);
            }

            if (string.IsNullOrWhiteSpace(notificationEmail)) return;

            await _emailService.SendGenericAsync(notificationEmail, subject, htmlBody, ct);
            _logger.LogInformation("Email notification sent to {Email} for agency {AgencyId}, project {ProjectId}: {Subject}",
                notificationEmail, agencyId, projectId, subject);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send email notification for agency {AgencyId}, project {ProjectId}", agencyId, projectId);
        }
    }
}
