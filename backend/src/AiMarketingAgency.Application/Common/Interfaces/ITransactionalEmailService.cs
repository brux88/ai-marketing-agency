namespace AiMarketingAgency.Application.Common.Interfaces;

public interface ITransactionalEmailService
{
    Task SendEmailConfirmationAsync(string toEmail, string fullName, string confirmationLink, CancellationToken ct = default);
    Task SendPasswordResetAsync(string toEmail, string fullName, string resetLink, CancellationToken ct = default);
    Task SendWelcomeAsync(string toEmail, string fullName, CancellationToken ct = default);
    Task SendAccountDeletionConfirmationAsync(string toEmail, string fullName, string confirmationLink, CancellationToken ct = default);
    Task SendTeamInvitationAsync(string toEmail, string inviterName, string teamName, string invitationLink, CancellationToken ct = default);
}
