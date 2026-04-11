using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Application.Content.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Application.Content.Queries.GetContent;

public class GetContentQueryHandler : IRequestHandler<GetContentQuery, List<ContentDto>>
{
    private readonly IAppDbContext _context;

    public GetContentQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ContentDto>> Handle(GetContentQuery request, CancellationToken cancellationToken)
    {
        var query = _context.GeneratedContents
            .Where(c => c.AgencyId == request.AgencyId);

        if (request.TypeFilter.HasValue)
            query = query.Where(c => c.ContentType == request.TypeFilter.Value);

        if (request.StatusFilter.HasValue)
            query = query.Where(c => c.Status == request.StatusFilter.Value);

        return await query
            .Select(c => new ContentDto
            {
                Id = c.Id,
                AgencyId = c.AgencyId,
                ContentType = c.ContentType,
                Title = c.Title,
                Body = c.Body,
                Status = c.Status,
                QualityScore = c.QualityScore,
                RelevanceScore = c.RelevanceScore,
                SeoScore = c.SeoScore,
                BrandVoiceScore = c.BrandVoiceScore,
                OverallScore = c.OverallScore,
                ScoreExplanation = c.ScoreExplanation,
                AutoApproved = c.AutoApproved,
                CreatedAt = c.CreatedAt,
                ApprovedAt = c.ApprovedAt,
                ImageUrl = c.ImageUrl,
                ImagePrompt = c.ImagePrompt,
                VideoUrl = c.VideoUrl
            })
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
