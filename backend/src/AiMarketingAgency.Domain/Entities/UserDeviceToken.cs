using AiMarketingAgency.Domain.Interfaces;

namespace AiMarketingAgency.Domain.Entities;

public class UserDeviceToken : BaseEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public string FcmToken { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty; // android | ios | web
    public string? DeviceName { get; set; }
    public DateTime LastSeenAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
