using System.Text.RegularExpressions;
using AiMarketingAgency.Domain.Enums;

namespace AiMarketingAgency.Application.Common;

public static class ContentPlatformResolver
{
    private static readonly Regex TagPattern = new(
        @"^\s*\[(?<tag>TWITTER|LINKEDIN|INSTAGRAM|FACEBOOK)\]",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static SocialPlatform? DetectFromTitle(string? title)
    {
        if (string.IsNullOrWhiteSpace(title)) return null;
        var match = TagPattern.Match(title);
        if (!match.Success) return null;
        return match.Groups["tag"].Value.ToUpperInvariant() switch
        {
            "TWITTER" => SocialPlatform.Twitter,
            "LINKEDIN" => SocialPlatform.LinkedIn,
            "INSTAGRAM" => SocialPlatform.Instagram,
            "FACEBOOK" => SocialPlatform.Facebook,
            _ => null
        };
    }
}
