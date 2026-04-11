using AiMarketingAgency.Domain.Enums;

namespace AiMarketingAgency.Application.Approvals.Dtos;

public class PendingApprovalDto
{
    public Guid Id { get; set; }
    public Guid AgencyId { get; set; }
    public Guid? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public ContentType ContentType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public ContentStatus Status { get; set; }
    public decimal OverallScore { get; set; }
    public string? ScoreExplanation { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}
