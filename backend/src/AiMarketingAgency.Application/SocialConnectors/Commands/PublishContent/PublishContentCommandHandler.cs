using System.Text.RegularExpressions;
using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AiMarketingAgency.Application.SocialConnectors.Commands.PublishContent;

public class PublishContentCommandHandler : IRequestHandler<PublishContentCommand, PublishResult>
{
    private readonly IAppDbContext _context;
    private readonly ISocialPublishingServiceFactory _factory;
    private readonly INotificationService _notificationService;
    private readonly IEmailNotificationService _emailNotificationService;
    private readonly IPushNotificationService _pushNotificationService;
    private readonly ILogger<PublishContentCommandHandler> _logger;

    public PublishContentCommandHandler(
        IAppDbContext context,
        ISocialPublishingServiceFactory factory,
        INotificationService notificationService,
        IEmailNotificationService emailNotificationService,
        IPushNotificationService pushNotificationService,
        ILogger<PublishContentCommandHandler> logger)
    {
        _context = context;
        _factory = factory;
        _notificationService = notificationService;
        _emailNotificationService = emailNotificationService;
        _pushNotificationService = pushNotificationService;
        _logger = logger;
    }

    public async Task<PublishResult> Handle(PublishContentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Publish request: agency={AgencyId} content={ContentId} platform={Platform}",
            request.AgencyId, request.ContentId, request.Platform);

        var content = await _context.GeneratedContents
            .FirstOrDefaultAsync(c => c.Id == request.ContentId && c.AgencyId == request.AgencyId, cancellationToken);
        if (content is null)
        {
            _logger.LogWarning("Publish aborted: content {ContentId} not found", request.ContentId);
            return new PublishResult(false, null, null, $"Content {request.ContentId} not found.");
        }

        SocialConnector? connector = null;
        if (content.ProjectId.HasValue)
        {
            connector = await _context.SocialConnectors.FirstOrDefaultAsync(
                c => c.AgencyId == request.AgencyId
                     && c.ProjectId == content.ProjectId
                     && c.Platform == request.Platform
                     && c.IsActive,
                cancellationToken);
        }
        connector ??= await _context.SocialConnectors.FirstOrDefaultAsync(
            c => c.AgencyId == request.AgencyId
                 && c.ProjectId == null
                 && c.Platform == request.Platform
                 && c.IsActive,
            cancellationToken);

        if (connector is null)
        {
            var msg = $"No active connector for platform {request.Platform}.";
            _logger.LogWarning(
                "Publish aborted: {Message} (agency={AgencyId}, project={ProjectId})",
                msg, request.AgencyId, content.ProjectId);
            return new PublishResult(false, null, null, msg);
        }

        string? projectUrl = null;
        if (content.ProjectId.HasValue)
        {
            projectUrl = await _context.Projects
                .AsNoTracking()
                .Where(p => p.Id == content.ProjectId.Value)
                .Select(p => p.WebsiteUrl)
                .FirstOrDefaultAsync(cancellationToken);
        }
        content.Body = SubstituteLinkPlaceholders(content.Body, projectUrl);
        content.Title = SubstituteLinkPlaceholders(content.Title, projectUrl);

        try
        {
            var publisher = _factory.Create(connector.Platform);
            var result = await publisher.PublishAsync(connector, content, cancellationToken);
            if (result.Success)
            {
                content.Status = Domain.Enums.ContentStatus.Published;
                content.PublishedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation(
                    "Publish OK: content={ContentId} platform={Platform} postUrl={PostUrl}",
                    content.Id, request.Platform, result.PostUrl);
            }
            else
                _logger.LogWarning(
                    "Publish failed: content={ContentId} platform={Platform} error={Error}",
                    content.Id, request.Platform, result.Error);

            try
            {
                await _notificationService.NotifyPublishResult(
                    content.TenantId, content.AgencyId, content.Id,
                    request.Platform.ToString(), result.Success, result.PostUrl);

                // Email notification on publication
                if (result.Success && content.ProjectId.HasValue)
                {
                    var project = await _context.Projects.AsNoTracking()
                        .FirstOrDefaultAsync(p => p.Id == content.ProjectId.Value, cancellationToken);

                    if (project?.NotifyEmailOnPublication == true && !string.IsNullOrWhiteSpace(project.NotificationEmail))
                    {
                        var emailSubject = $"Contenuto pubblicato su {request.Platform} - {content.Title}";
                        var emailHtml = $"""
                            <h2>Contenuto pubblicato</h2>
                            <p>Il contenuto <strong>{content.Title}</strong> e stato pubblicato su <strong>{request.Platform}</strong>.</p>
                            {(string.IsNullOrEmpty(result.PostUrl) ? "" : $"<p><a href=\"{result.PostUrl}\">Vedi il post</a></p>")}
                            """;
                        await _emailNotificationService.SendEmailNotificationAsync(
                            content.AgencyId, content.ProjectId, emailSubject, emailHtml, cancellationToken);
                    }

                    await _pushNotificationService.SendToProjectAsync(
                        content.AgencyId,
                        content.ProjectId,
                        PushEventType.ContentPublished,
                        $"Pubblicato su {request.Platform}",
                        content.Title,
                        new Dictionary<string, string>
                        {
                            ["agencyId"] = content.AgencyId.ToString(),
                            ["projectId"] = content.ProjectId.Value.ToString(),
                            ["contentId"] = content.Id.ToString(),
                            ["platform"] = request.Platform.ToString(),
                            ["event"] = "content.published",
                            ["postUrl"] = result.PostUrl ?? string.Empty,
                        },
                        cancellationToken);
                }
            }
            catch (Exception notifyEx)
            {
                _logger.LogWarning(notifyEx, "Failed to send publish notification for content {ContentId}", content.Id);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Publish exception: content={ContentId} platform={Platform}",
                content.Id, request.Platform);
            try
            {
                await _notificationService.NotifyPublishResult(
                    content.TenantId, content.AgencyId, content.Id,
                    request.Platform.ToString(), false, null);
            }
            catch { }
            return new PublishResult(false, null, null, ex.Message);
        }
    }

    private static readonly Regex LinkPlaceholder = new(
        @"\[\s*(link\s*demo|link|url|website|sito|sito\s*web|tuo\s*link|your\s*link|insert\s*link|cta\s*link)\s*\]",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static string SubstituteLinkPlaceholders(string text, string? projectUrl)
    {
        if (string.IsNullOrEmpty(text)) return text;
        var replacement = string.IsNullOrWhiteSpace(projectUrl) ? string.Empty : projectUrl!;
        var result = LinkPlaceholder.Replace(text, replacement);
        return Regex.Replace(result, @"[ \t]+\n", "\n").Trim();
    }
}
