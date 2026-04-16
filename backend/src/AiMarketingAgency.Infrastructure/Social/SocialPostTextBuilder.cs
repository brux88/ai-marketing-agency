using System.Text.RegularExpressions;
using AiMarketingAgency.Domain.Entities;
using AiMarketingAgency.Domain.Enums;

namespace AiMarketingAgency.Infrastructure.Social;

internal static class SocialPostTextBuilder
{
    private static readonly Regex PlatformTagPrefix = new(
        @"^\s*\[(TWITTER|LINKEDIN|INSTAGRAM|FACEBOOK)\]\s*",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex LinkPlaceholder = new(
        @"\[\s*(link\s*demo|link|url|website|sito|sito\s*web|tuo\s*link|your\s*link|insert\s*link|cta\s*link)\s*\]",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static string Build(GeneratedContent content, string? projectUrl = null)
    {
        var body = SubstituteLinkPlaceholders(content.Body ?? string.Empty, projectUrl);
        if (content.ContentType == ContentType.SocialPost) return body.Trim();

        var title = PlatformTagPrefix.Replace(content.Title ?? string.Empty, string.Empty).Trim();
        title = SubstituteLinkPlaceholders(title, projectUrl);
        return string.IsNullOrEmpty(title) ? body.Trim() : $"{title}\n\n{body}".Trim();
    }

    private static string SubstituteLinkPlaceholders(string text, string? projectUrl)
    {
        if (string.IsNullOrEmpty(text)) return text;
        var replacement = string.IsNullOrWhiteSpace(projectUrl) ? string.Empty : projectUrl!;
        var result = LinkPlaceholder.Replace(text, replacement);
        return Regex.Replace(result, @"[ \t]+\n", "\n").Trim();
    }
}
