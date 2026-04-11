using AiMarketingAgency.Application.Approvals.Dtos;
using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Application.Approvals.Queries.GetPendingApprovals;

public class GetPendingApprovalsQueryHandler : IRequestHandler<GetPendingApprovalsQuery, List<PendingApprovalDto>>
{
    private readonly IAppDbContext _context;

    public GetPendingApprovalsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<PendingApprovalDto>> Handle(GetPendingApprovalsQuery request, CancellationToken cancellationToken)
    {
        return await _context.GeneratedContents
            .Where(c => c.AgencyId == request.AgencyId
                && (c.Status == ContentStatus.InReview || c.Status == ContentStatus.Draft))
            .Include(c => c.Project)
            .Select(c => new PendingApprovalDto
            {
                Id = c.Id,
                AgencyId = c.AgencyId,
                ProjectId = c.ProjectId,
                ProjectName = c.Project != null ? c.Project.Name : null,
                ContentType = c.ContentType,
                Title = c.Title,
                Body = c.Body,
                Status = c.Status,
                OverallScore = c.OverallScore,
                ScoreExplanation = c.ScoreExplanation,
                ImageUrl = c.ImageUrl,
                CreatedAt = c.CreatedAt
            })
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
