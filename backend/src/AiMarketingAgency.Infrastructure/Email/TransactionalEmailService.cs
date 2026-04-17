using AiMarketingAgency.Application.Common.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace AiMarketingAgency.Infrastructure.Email;

public class TransactionalEmailService : ITransactionalEmailService
{
    private readonly TransactionalEmailOptions _options;
    private readonly ILogger<TransactionalEmailService> _logger;

    public TransactionalEmailService(IOptions<TransactionalEmailOptions> options, ILogger<TransactionalEmailService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendEmailConfirmationAsync(string toEmail, string fullName, string confirmationLink, CancellationToken ct = default)
    {
        var html = EmailTemplates.EmailConfirmation(fullName, confirmationLink);
        await SendAsync(_options.NoReplyEmail, _options.NoReplyPassword, toEmail, "Conferma il tuo indirizzo email - WePost AI", html, ct);
        _logger.LogInformation("Confirmation email sent to {Email}", toEmail);
    }

    public async Task SendPasswordResetAsync(string toEmail, string fullName, string resetLink, CancellationToken ct = default)
    {
        var html = EmailTemplates.PasswordReset(fullName, resetLink);
        await SendAsync(_options.NoReplyEmail, _options.NoReplyPassword, toEmail, "Reimposta la tua password - WePost AI", html, ct);
        _logger.LogInformation("Password reset email sent to {Email}", toEmail);
    }

    public async Task SendWelcomeAsync(string toEmail, string fullName, CancellationToken ct = default)
    {
        var html = EmailTemplates.Welcome(fullName);
        await SendAsync(_options.NoReplyEmail, _options.NoReplyPassword, toEmail, "Benvenuto su WePost AI!", html, ct);
        _logger.LogInformation("Welcome email sent to {Email}", toEmail);
    }

    public async Task SendAccountDeletionConfirmationAsync(string toEmail, string fullName, string confirmationLink, CancellationToken ct = default)
    {
        var html = EmailTemplates.AccountDeletionConfirmation(fullName, confirmationLink);
        await SendAsync(_options.NoReplyEmail, _options.NoReplyPassword, toEmail, "Conferma eliminazione account - WePost AI", html, ct);
        _logger.LogInformation("Account deletion confirmation email sent to {Email}", toEmail);
    }

    public async Task SendTeamInvitationAsync(string toEmail, string inviterName, string teamName, string invitationLink, CancellationToken ct = default)
    {
        var html = EmailTemplates.TeamInvitation(inviterName, teamName, invitationLink);
        await SendAsync(_options.NoReplyEmail, _options.NoReplyPassword, toEmail, $"Invito al team {teamName} - WePost AI", html, ct);
        _logger.LogInformation("Team invitation email sent to {Email}", toEmail);
    }

    public async Task SendGenericAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default)
    {
        await SendAsync(_options.NoReplyEmail, _options.NoReplyPassword, toEmail, subject, htmlBody, ct);
        _logger.LogInformation("Generic email sent to {Email}: {Subject}", toEmail, subject);
    }

    private async Task SendAsync(string fromEmail, string fromPassword, string toEmail, string subject, string htmlBody, CancellationToken ct)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.SenderName, fromEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_options.SmtpHost, _options.SmtpPort, SecureSocketOptions.SslOnConnect, ct);
        await client.AuthenticateAsync(fromEmail, fromPassword, ct);
        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);
    }
}
