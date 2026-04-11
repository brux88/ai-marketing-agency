namespace AiMarketingAgency.Application.Newsletter.Dtos;

public class SubscriberDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }
    public bool IsActive { get; set; }
    public DateTime SubscribedAt { get; set; }
}
