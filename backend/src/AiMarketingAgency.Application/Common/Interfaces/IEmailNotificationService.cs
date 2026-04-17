namespace AiMarketingAgency.Application.Common.Interfaces;

public interface IEmailNotificationService
{
    Task SendEmailNotificationAsync(Guid agencyId, Guid? projectId, string subject, string htmlBody, CancellationToken ct = default);
}
