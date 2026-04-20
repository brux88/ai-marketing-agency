using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Domain.Entities;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AiMarketingAgency.Infrastructure.Notifications;

public class FcmPushNotificationService : IPushNotificationService
{
    private static readonly object _initLock = new();
    private static bool _initAttempted;
    private static FirebaseApp? _app;

    private readonly IAppDbContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<FcmPushNotificationService> _logger;
    private readonly IConfiguration _configuration;

    public FcmPushNotificationService(
        IAppDbContext context,
        ITenantContext tenantContext,
        IConfiguration configuration,
        ILogger<FcmPushNotificationService> logger)
    {
        _context = context;
        _tenantContext = tenantContext;
        _configuration = configuration;
        _logger = logger;
        EnsureInitialized();
    }

    private void EnsureInitialized()
    {
        if (_initAttempted) return;
        lock (_initLock)
        {
            if (_initAttempted) return;
            _initAttempted = true;
            try
            {
                var credentialsPath = _configuration["Firebase:CredentialsPath"]
                    ?? Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
                var credentialsJson = _configuration["Firebase:CredentialsJson"]
                    ?? Environment.GetEnvironmentVariable("FIREBASE_CREDENTIALS_JSON");

                GoogleCredential? credential = null;
                if (!string.IsNullOrWhiteSpace(credentialsJson))
                {
                    credential = GoogleCredential.FromJson(credentialsJson);
                }
                else if (!string.IsNullOrWhiteSpace(credentialsPath) && File.Exists(credentialsPath))
                {
                    credential = GoogleCredential.FromFile(credentialsPath);
                }

                if (credential == null)
                {
                    _logger.LogInformation("Firebase credentials not configured; push notifications disabled.");
                    return;
                }

                _app = FirebaseApp.DefaultInstance
                    ?? FirebaseApp.Create(new AppOptions { Credential = credential });
                _logger.LogInformation("Firebase Admin initialized for push notifications.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Firebase Admin init failed; push notifications disabled.");
            }
        }
    }

    private bool IsEnabled => _app != null;

    public async Task SendToProjectAsync(
        Guid agencyId,
        Guid? projectId,
        PushEventType eventType,
        string title,
        string body,
        IReadOnlyDictionary<string, string>? data = null,
        CancellationToken ct = default)
    {
        try
        {
            if (!projectId.HasValue) return;

            var flags = await _context.Projects
                .AsNoTracking()
                .Where(p => p.Id == projectId.Value && p.AgencyId == agencyId)
                .Select(p => new
                {
                    p.TenantId,
                    p.NotifyPushOnGeneration,
                    p.NotifyPushOnPublication,
                    p.NotifyPushOnApprovalNeeded,
                })
                .FirstOrDefaultAsync(ct);
            if (flags == null) return;

            var shouldSend = eventType switch
            {
                PushEventType.ContentGenerated => flags.NotifyPushOnGeneration,
                PushEventType.ContentPublished => flags.NotifyPushOnPublication,
                PushEventType.ApprovalNeeded => flags.NotifyPushOnApprovalNeeded,
                _ => false,
            };
            if (!shouldSend) return;

            var tokens = await _context.UserDeviceTokens
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(t => t.TenantId == flags.TenantId)
                .Where(t => _context.Users
                    .IgnoreQueryFilters()
                    .Any(u => u.Id == t.UserId
                        && u.TenantId == flags.TenantId
                        && (u.AllowedAgencyIds == null
                            || u.AllowedAgencyIds.Contains(agencyId.ToString()))))
                .Select(t => t.FcmToken)
                .ToListAsync(ct);

            if (tokens.Count == 0) return;

            if (!IsEnabled)
            {
                _logger.LogInformation("Push event {EventType} matched {Count} tokens but Firebase is not configured.",
                    eventType, tokens.Count);
                return;
            }

            var messaging = FirebaseMessaging.DefaultInstance;
            var message = new MulticastMessage
            {
                Tokens = tokens,
                Notification = new FirebaseAdmin.Messaging.Notification { Title = title, Body = body },
                Data = data?.ToDictionary(kv => kv.Key, kv => kv.Value) ?? new Dictionary<string, string>(),
                Android = new AndroidConfig { Priority = Priority.High },
                Apns = new ApnsConfig { Aps = new Aps { Sound = "default" } },
            };
            var response = await messaging.SendEachForMulticastAsync(message, ct);
            _logger.LogInformation("FCM push sent: {Success} ok, {Fail} failed (event {Event})",
                response.SuccessCount, response.FailureCount, eventType);

            if (response.FailureCount > 0)
            {
                var staleTokens = new List<string>();
                for (var i = 0; i < response.Responses.Count; i++)
                {
                    var r = response.Responses[i];
                    if (r.IsSuccess) continue;
                    var code = r.Exception?.MessagingErrorCode;
                    if (code == MessagingErrorCode.Unregistered || code == MessagingErrorCode.InvalidArgument)
                    {
                        staleTokens.Add(tokens[i]);
                    }
                }
                if (staleTokens.Count > 0)
                {
                    var stale = await _context.UserDeviceTokens
                        .IgnoreQueryFilters()
                        .Where(t => staleTokens.Contains(t.FcmToken))
                        .ToListAsync(ct);
                    foreach (var t in stale) _context.UserDeviceTokens.Remove(t);
                    await _context.SaveChangesAsync(ct);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send push notification for agency {AgencyId} project {ProjectId}",
                agencyId, projectId);
        }
    }

    public async Task RegisterTokenAsync(Guid userId, string fcmToken, string platform, string? deviceName, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(fcmToken)) return;
        var existing = await _context.UserDeviceTokens
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.FcmToken == fcmToken, ct);
        if (existing != null)
        {
            existing.UserId = userId;
            existing.Platform = platform;
            existing.DeviceName = deviceName;
            existing.LastSeenAt = DateTime.UtcNow;
        }
        else
        {
            _context.UserDeviceTokens.Add(new UserDeviceToken
            {
                TenantId = _tenantContext.TenantId,
                UserId = userId,
                FcmToken = fcmToken,
                Platform = platform,
                DeviceName = deviceName,
                LastSeenAt = DateTime.UtcNow,
            });
        }
        await _context.SaveChangesAsync(ct);
    }

    public async Task UnregisterTokenAsync(Guid userId, string fcmToken, CancellationToken ct = default)
    {
        var existing = await _context.UserDeviceTokens
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.FcmToken == fcmToken && t.UserId == userId, ct);
        if (existing != null)
        {
            _context.UserDeviceTokens.Remove(existing);
            await _context.SaveChangesAsync(ct);
        }
    }
}
