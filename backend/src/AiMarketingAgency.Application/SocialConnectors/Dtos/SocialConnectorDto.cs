using AiMarketingAgency.Domain.Enums;

namespace AiMarketingAgency.Application.SocialConnectors.Dtos;

public class SocialConnectorDto
{
    public Guid Id { get; set; }
    public Guid? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public SocialPlatform Platform { get; set; }
    public string PlatformName => Platform.ToString();
    public string? AccountId { get; set; }
    public string? AccountName { get; set; }
    public string? ProfileImageUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime? TokenExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
