using System.Text;
using System.Text.Json;
using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Domain.Entities;
using AiMarketingAgency.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AiMarketingAgency.Infrastructure.Social;

public class MetaPublisher : ISocialPublishingService
{
    private readonly HttpClient _httpClient;
    private readonly SocialPlatform _platform;
    private readonly IConfiguration? _configuration;
    private readonly ILogger<MetaPublisher>? _logger;

    public MetaPublisher(HttpClient httpClient, SocialPlatform platform, IConfiguration? configuration = null, ILogger<MetaPublisher>? logger = null)
    {
        _httpClient = httpClient;
        _platform = platform;
        _configuration = configuration;
        _logger = logger;
    }

    private string? ResolveAbsoluteImageUrl(string? imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl)) return null;
        if (Uri.TryCreate(imageUrl, UriKind.Absolute, out var abs) && (abs.Scheme == "http" || abs.Scheme == "https"))
            return imageUrl;
        var publicBaseUrl = _configuration?["PublicBaseUrl"]?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(publicBaseUrl))
        {
            _logger?.LogWarning(
                "Image URL '{Url}' is relative and PublicBaseUrl is not configured. Meta requires publicly reachable URLs. Falling back to text-only post. Set 'PublicBaseUrl' in appsettings.json (e.g. ngrok URL) to enable image publishing.",
                imageUrl);
            return null;
        }
        var slash = imageUrl.StartsWith('/') ? "" : "/";
        return $"{publicBaseUrl}{slash}{imageUrl}";
    }

    private static string? ResolveLocalImagePath(string? imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl)) return null;
        if (imageUrl.StartsWith("/generated-images/") || imageUrl.StartsWith("generated-images/"))
        {
            var webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var localPath = Path.Combine(webRoot, imageUrl.TrimStart('/'));
            return localPath;
        }
        return null;
    }

    public async Task<PublishResult> PublishAsync(SocialConnector connector, GeneratedContent content, CancellationToken ct)
    {
        try
        {
            return _platform switch
            {
                SocialPlatform.Instagram => await PublishToInstagramAsync(connector, content, ct),
                SocialPlatform.Facebook => await PublishToFacebookAsync(connector, content, ct),
                _ => new PublishResult(false, null, null, $"Unsupported Meta platform: {_platform}")
            };
        }
        catch (Exception ex)
        {
            return new PublishResult(false, null, null, ex.Message);
        }
    }

    private async Task<PublishResult> PublishToInstagramAsync(SocialConnector connector, GeneratedContent content, CancellationToken ct)
    {
        var igUserId = connector.AccountId;
        var accessToken = connector.AccessToken;

        // Step 1: Create media container
        var createUrl = $"https://graph.facebook.com/v19.0/{igUserId}/media";
        var createParams = new Dictionary<string, string>
        {
            ["caption"] = SocialPostTextBuilder.Build(content),
            ["access_token"] = accessToken
        };

        var resolvedIgImageUrl = await ResolveInstagramImageUrlAsync(content, accessToken, ct);
        if (string.IsNullOrEmpty(resolvedIgImageUrl))
        {
            return new PublishResult(false, null, null,
                "Instagram richiede un'immagine con URL pubblico raggiungibile. " +
                "Verifica che l'immagine esista o configura 'PublicBaseUrl' in appsettings.json.");
        }
        createParams["image_url"] = resolvedIgImageUrl;

        var createResponse = await _httpClient.PostAsync(createUrl,
            new FormUrlEncodedContent(createParams), ct);
        var createBody = await createResponse.Content.ReadAsStringAsync(ct);

        // Meta's crawler sometimes fails to fetch otherwise-public URLs (Azure Blob, CDNs).
        // Error 9004 / sub-error 2207052 = "Only photo or video can be accepted as media type",
        // which Meta returns when it can't read the remote media. Fall back to re-hosting the
        // image on Facebook (which gives us a fb CDN URL Meta always trusts) and retry once.
        if (!createResponse.IsSuccessStatusCode && IsMediaUnreachableError(createBody))
        {
            _logger?.LogWarning(
                "Instagram rejected image {Url} with fetch error; re-hosting via Facebook and retrying",
                resolvedIgImageUrl);

            var rehosted = await RehostRemoteImageViaFacebookAsync(resolvedIgImageUrl, accessToken, ct);
            if (!string.IsNullOrEmpty(rehosted) && !string.Equals(rehosted, resolvedIgImageUrl, StringComparison.OrdinalIgnoreCase))
            {
                createParams["image_url"] = rehosted;
                createResponse = await _httpClient.PostAsync(createUrl,
                    new FormUrlEncodedContent(createParams), ct);
                createBody = await createResponse.Content.ReadAsStringAsync(ct);
            }
        }

        if (!createResponse.IsSuccessStatusCode)
            return new PublishResult(false, null, null, $"Instagram create error: {createBody}");

        using var createDoc = JsonDocument.Parse(createBody);
        var containerId = createDoc.RootElement.GetProperty("id").GetString();

        // Step 1b: Poll container status until FINISHED (Meta API requires media to be ready before publish)
        var waitResult = await WaitForContainerReadyAsync(containerId!, accessToken, ct);
        if (!waitResult.Ready)
            return new PublishResult(false, null, null, $"Instagram container not ready: {waitResult.Error}");

        // Step 2: Publish the container
        var publishUrl = $"https://graph.facebook.com/v19.0/{igUserId}/media_publish";
        var publishParams = new Dictionary<string, string>
        {
            ["creation_id"] = containerId!,
            ["access_token"] = accessToken
        };

        var publishResponse = await _httpClient.PostAsync(publishUrl,
            new FormUrlEncodedContent(publishParams), ct);
        var publishBody = await publishResponse.Content.ReadAsStringAsync(ct);

        if (!publishResponse.IsSuccessStatusCode)
            return new PublishResult(false, null, null, $"Instagram publish error: {publishBody}");

        using var publishDoc = JsonDocument.Parse(publishBody);
        var mediaId = publishDoc.RootElement.GetProperty("id").GetString();

        return new PublishResult(true, mediaId, $"https://www.instagram.com/p/{mediaId}/", null);
    }

    private async Task<PublishResult> PublishToFacebookAsync(SocialConnector connector, GeneratedContent content, CancellationToken ct)
    {
        var pageId = connector.AccountId;
        var accessToken = connector.AccessToken;
        var message = SocialPostTextBuilder.Build(content);

        var resolvedFbImageUrl = ResolveAbsoluteImageUrl(content.ImageUrl);

        // Try binary upload from local file when image is local and PublicBaseUrl is missing
        if (string.IsNullOrEmpty(resolvedFbImageUrl) && !string.IsNullOrEmpty(content.ImageUrl))
        {
            var localPath = ResolveLocalImagePath(content.ImageUrl);
            if (localPath != null && File.Exists(localPath))
            {
                return await PublishFacebookWithBinaryUploadAsync(pageId, accessToken, message, localPath, ct);
            }
        }

        var url = $"https://graph.facebook.com/v19.0/{pageId}/feed";
        var postParams = new Dictionary<string, string>
        {
            ["message"] = message,
            ["access_token"] = accessToken
        };

        if (!string.IsNullOrEmpty(resolvedFbImageUrl))
        {
            url = $"https://graph.facebook.com/v19.0/{pageId}/photos";
            postParams["url"] = resolvedFbImageUrl;
        }

        var response = await _httpClient.PostAsync(url, new FormUrlEncodedContent(postParams), ct);
        var responseBody = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
            return new PublishResult(false, null, null, $"Facebook API error: {responseBody}");

        using var doc = JsonDocument.Parse(responseBody);
        var postId = doc.RootElement.GetProperty("id").GetString();

        return new PublishResult(true, postId, $"https://www.facebook.com/{postId}", null);
    }

    private async Task<PublishResult> PublishFacebookWithBinaryUploadAsync(
        string pageId, string accessToken, string message, string localPath, CancellationToken ct)
    {
        var url = $"https://graph.facebook.com/v19.0/{pageId}/photos";
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(message), "message");
        form.Add(new StringContent(accessToken), "access_token");

        var imageBytes = await File.ReadAllBytesAsync(localPath, ct);
        var imageContent = new ByteArrayContent(imageBytes);
        imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
        form.Add(imageContent, "source", Path.GetFileName(localPath));

        var response = await _httpClient.PostAsync(url, form, ct);
        var responseBody = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
            return new PublishResult(false, null, null, $"Facebook API error: {responseBody}");

        using var doc = JsonDocument.Parse(responseBody);
        var postId = doc.RootElement.GetProperty("id").GetString();
        return new PublishResult(true, postId, $"https://www.facebook.com/{postId}", null);
    }

    // Instagram requires a publicly fetchable image URL. Our own server URLs are sometimes
    // unreachable from Meta's crawler (file was deleted, CDN cache, redeploys). For robustness
    // we always re-host images hosted on our server onto Facebook first. External URLs
    // (DALL-E, Azure Blob, etc.) are used as-is.
    private async Task<string?> ResolveInstagramImageUrlAsync(GeneratedContent content, string accessToken, CancellationToken ct)
    {
        async Task<string?> TryCandidateAsync(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;

            if (Uri.TryCreate(url, UriKind.Absolute, out var abs)
                && (abs.Scheme == "http" || abs.Scheme == "https"))
            {
                var publicBaseUrl = _configuration?["PublicBaseUrl"]?.TrimEnd('/');
                if (!string.IsNullOrWhiteSpace(publicBaseUrl)
                    && url.StartsWith(publicBaseUrl, StringComparison.OrdinalIgnoreCase))
                {
                    // URL points at our own server — re-host through Facebook for reliability
                    var localPath = TryLocateLocalFile(url, publicBaseUrl);
                    if (localPath != null && File.Exists(localPath))
                    {
                        _logger?.LogInformation("Instagram: re-hosting server-local image via Facebook for reliable fetch");
                        return await UploadToFacebookForUrlAsync(accessToken, localPath, ct);
                    }
                    _logger?.LogWarning("Instagram: stored image URL points at our server but local file is missing: {Url}", url);
                    return null;
                }
                return url;
            }

            // Relative path — try to locate local file and re-host via Facebook
            var local = ResolveLocalImagePath(url);
            if (local != null && File.Exists(local))
            {
                _logger?.LogInformation("Instagram: overlay image is local, uploading to Facebook to get public URL");
                return await UploadToFacebookForUrlAsync(accessToken, local, ct);
            }

            return ResolveAbsoluteImageUrl(url);
        }

        var primary = await TryCandidateAsync(content.ImageUrl);
        if (!string.IsNullOrEmpty(primary)) return primary;

        _logger?.LogWarning("Instagram: primary image {Url} unreachable, falling back to OriginalImageUrl", content.ImageUrl);
        return await TryCandidateAsync(content.OriginalImageUrl);
    }

    private static string? TryLocateLocalFile(string absoluteUrl, string publicBaseUrl)
    {
        if (!absoluteUrl.StartsWith(publicBaseUrl, StringComparison.OrdinalIgnoreCase)) return null;
        var path = absoluteUrl[publicBaseUrl.Length..].TrimStart('/');
        var webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        return Path.Combine(webRoot, path.Replace('/', Path.DirectorySeparatorChar));
    }

    private async Task<string?> UploadToFacebookForUrlAsync(string accessToken, string localPath, CancellationToken ct)
    {
        try
        {
            var meResp = await _httpClient.GetAsync($"https://graph.facebook.com/v19.0/me?fields=id&access_token={accessToken}", ct);
            var meBody = await meResp.Content.ReadAsStringAsync(ct);
            if (!meResp.IsSuccessStatusCode) return null;

            using var meDoc = JsonDocument.Parse(meBody);
            var pageId = meDoc.RootElement.GetProperty("id").GetString();
            if (string.IsNullOrEmpty(pageId)) return null;

            var url = $"https://graph.facebook.com/v19.0/{pageId}/photos";
            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(accessToken), "access_token");
            form.Add(new StringContent("false"), "published");

            var imageBytes = await File.ReadAllBytesAsync(localPath, ct);
            var imageContent = new ByteArrayContent(imageBytes);
            imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
            form.Add(imageContent, "source", System.IO.Path.GetFileName(localPath));

            var uploadResp = await _httpClient.PostAsync(url, form, ct);
            var uploadBody = await uploadResp.Content.ReadAsStringAsync(ct);
            if (!uploadResp.IsSuccessStatusCode)
            {
                _logger?.LogWarning("Failed to upload overlay to Facebook for Instagram URL: {Body}", uploadBody);
                return null;
            }

            using var uploadDoc = JsonDocument.Parse(uploadBody);
            var photoId = uploadDoc.RootElement.GetProperty("id").GetString();

            var imgResp = await _httpClient.GetAsync(
                $"https://graph.facebook.com/v19.0/{photoId}?fields=images&access_token={accessToken}", ct);
            var imgBody = await imgResp.Content.ReadAsStringAsync(ct);
            if (!imgResp.IsSuccessStatusCode) return null;

            using var imgDoc = JsonDocument.Parse(imgBody);
            if (imgDoc.RootElement.TryGetProperty("images", out var images) && images.GetArrayLength() > 0)
            {
                var largest = images[0];
                var sourceUrl = largest.GetProperty("source").GetString();
                _logger?.LogInformation("Uploaded overlay to Facebook, got public URL for Instagram: {Url}", sourceUrl);
                return sourceUrl;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to upload overlay image to Facebook for Instagram");
        }
        return null;
    }

    private static bool IsMediaUnreachableError(string responseBody)
    {
        if (string.IsNullOrEmpty(responseBody)) return false;
        return responseBody.Contains("2207052", StringComparison.Ordinal)
            || responseBody.Contains("\"code\":9004", StringComparison.Ordinal)
            || responseBody.Contains("Only photo or video can be accepted", StringComparison.OrdinalIgnoreCase)
            || responseBody.Contains("Media Fetch Failure", StringComparison.OrdinalIgnoreCase);
    }

    private async Task<string?> RehostRemoteImageViaFacebookAsync(string remoteUrl, string accessToken, CancellationToken ct)
    {
        try
        {
            var downloadResp = await _httpClient.GetAsync(remoteUrl, ct);
            if (!downloadResp.IsSuccessStatusCode)
            {
                _logger?.LogWarning("Could not download remote image for rehost: {Status}", downloadResp.StatusCode);
                return null;
            }
            var bytes = await downloadResp.Content.ReadAsByteArrayAsync(ct);
            var contentType = downloadResp.Content.Headers.ContentType?.MediaType ?? "image/png";
            var fileName = Path.GetFileName(new Uri(remoteUrl).LocalPath);
            if (string.IsNullOrWhiteSpace(fileName)) fileName = "image.png";

            var meResp = await _httpClient.GetAsync(
                $"https://graph.facebook.com/v19.0/me?fields=id&access_token={accessToken}", ct);
            var meBody = await meResp.Content.ReadAsStringAsync(ct);
            if (!meResp.IsSuccessStatusCode) return null;
            using var meDoc = JsonDocument.Parse(meBody);
            var pageId = meDoc.RootElement.GetProperty("id").GetString();
            if (string.IsNullOrEmpty(pageId)) return null;

            var uploadUrl = $"https://graph.facebook.com/v19.0/{pageId}/photos";
            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(accessToken), "access_token");
            form.Add(new StringContent("false"), "published");
            var imageContent = new ByteArrayContent(bytes);
            imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            form.Add(imageContent, "source", fileName);

            var uploadResp = await _httpClient.PostAsync(uploadUrl, form, ct);
            var uploadBody = await uploadResp.Content.ReadAsStringAsync(ct);
            if (!uploadResp.IsSuccessStatusCode)
            {
                _logger?.LogWarning("Rehost upload to Facebook failed: {Body}", uploadBody);
                return null;
            }
            using var uploadDoc = JsonDocument.Parse(uploadBody);
            var photoId = uploadDoc.RootElement.GetProperty("id").GetString();

            var imgResp = await _httpClient.GetAsync(
                $"https://graph.facebook.com/v19.0/{photoId}?fields=images&access_token={accessToken}", ct);
            var imgBody = await imgResp.Content.ReadAsStringAsync(ct);
            if (!imgResp.IsSuccessStatusCode) return null;
            using var imgDoc = JsonDocument.Parse(imgBody);
            if (imgDoc.RootElement.TryGetProperty("images", out var images) && images.GetArrayLength() > 0)
            {
                var src = images[0].GetProperty("source").GetString();
                _logger?.LogInformation("Rehosted Instagram image via Facebook CDN: {Url}", src);
                return src;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "RehostRemoteImageViaFacebookAsync failed for {Url}", remoteUrl);
        }
        return null;
    }

    private async Task<(bool Ready, string? Error)> WaitForContainerReadyAsync(
        string containerId, string accessToken, CancellationToken ct)
    {
        // Meta docs: poll every ~3s, finished when status_code == FINISHED.
        // IN_PROGRESS/PUBLISHED/ERROR/EXPIRED are other possible states.
        const int maxAttempts = 15;
        const int delayMs = 3000;
        var statusUrl = $"https://graph.facebook.com/v19.0/{containerId}?fields=status_code,status&access_token={accessToken}";

        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            try
            {
                var response = await _httpClient.GetAsync(statusUrl, ct);
                var body = await response.Content.ReadAsStringAsync(ct);
                if (response.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(body);
                    var code = doc.RootElement.TryGetProperty("status_code", out var sc) ? sc.GetString() : null;
                    if (string.Equals(code, "FINISHED", StringComparison.OrdinalIgnoreCase))
                        return (true, null);
                    if (string.Equals(code, "ERROR", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(code, "EXPIRED", StringComparison.OrdinalIgnoreCase))
                    {
                        var status = doc.RootElement.TryGetProperty("status", out var s) ? s.GetString() : code;
                        return (false, status ?? code);
                    }
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // transient — keep polling
            }

            await Task.Delay(delayMs, ct);
        }

        return (false, "timed out after 45s waiting for FINISHED status");
    }
}
