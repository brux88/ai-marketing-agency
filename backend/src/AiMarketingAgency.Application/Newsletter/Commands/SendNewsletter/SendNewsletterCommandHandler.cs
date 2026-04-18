using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Application.Newsletter.Commands.SendNewsletter;

public class SendNewsletterCommandHandler : IRequestHandler<SendNewsletterCommand, EmailSendResult>
{
    private readonly IAppDbContext _context;
    private readonly IEmailSendingService _emailService;

    public SendNewsletterCommandHandler(IAppDbContext context, IEmailSendingService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task<EmailSendResult> Handle(SendNewsletterCommand request, CancellationToken cancellationToken)
    {
        var content = await _context.GeneratedContents
            .FirstOrDefaultAsync(c => c.Id == request.ContentId && c.AgencyId == request.AgencyId, cancellationToken)
            ?? throw new KeyNotFoundException("Content not found.");

        // Prefer project-specific connector, fall back to agency default (ProjectId == null)
        EmailConnector? emailConnector = null;
        if (content.ProjectId.HasValue)
        {
            emailConnector = await _context.EmailConnectors
                .FirstOrDefaultAsync(
                    c => c.AgencyId == request.AgencyId
                         && c.ProjectId == content.ProjectId
                         && c.IsActive,
                    cancellationToken);
        }

        emailConnector ??= await _context.EmailConnectors
            .FirstOrDefaultAsync(
                c => c.AgencyId == request.AgencyId && c.ProjectId == null && c.IsActive,
                cancellationToken)
            ?? throw new InvalidOperationException("No email connector configured for this agency or project.");

        // Load project-specific subscribers if content belongs to a project, otherwise agency-level
        var subscribersQuery = _context.NewsletterSubscribers
            .Where(s => s.AgencyId == request.AgencyId && s.IsActive);
        if (content.ProjectId.HasValue)
            subscribersQuery = subscribersQuery.Where(s => s.ProjectId == content.ProjectId.Value);
        else
            subscribersQuery = subscribersQuery.Where(s => s.ProjectId == null);

        var subscribers = await subscribersQuery.ToListAsync(cancellationToken);

        if (subscribers.Count == 0)
            return new EmailSendResult(true, 0, 0, "No active subscribers.");

        return await _emailService.SendNewsletterAsync(
            emailConnector,
            subscribers,
            content.Title,
            content.Body,
            cancellationToken);
    }
}
