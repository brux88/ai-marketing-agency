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
            if (!projectId.HasValue) return;

            var project = await _context.Projects
                .AsNoTracking()
                .Where(p => p.Id == projectId.Value && p.AgencyId == agencyId)
                .Select(p => new { p.NotificationEmail, p.NotifyEmailOnGeneration, p.NotifyEmailOnPublication, p.NotifyEmailOnApprovalNeeded })
                .FirstOrDefaultAsync(ct);

            if (project == null || string.IsNullOrWhiteSpace(project.NotificationEmail))
                return;

            await _emailService.SendGenericAsync(project.NotificationEmail, subject, htmlBody, ct);
            _logger.LogInformation("Email notification sent to {Email} for project {ProjectId}: {Subject}",
                project.NotificationEmail, projectId, subject);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send email notification for agency {AgencyId}, project {ProjectId}", agencyId, projectId);
        }
    }
}
