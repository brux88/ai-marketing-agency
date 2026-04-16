using System.Text.Json;
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

        if (request.ProjectId.HasValue)
            query = query.Where(c => c.ProjectId == request.ProjectId.Value);

        var rows = await query
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new
            {
                c.Id,
                c.AgencyId,
                c.ContentType,
                c.Title,
                c.Body,
                c.Status,
                c.QualityScore,
                c.RelevanceScore,
                c.SeoScore,
                c.BrandVoiceScore,
                c.OverallScore,
                c.ScoreExplanation,
                c.AutoApproved,
                c.CreatedAt,
                c.ApprovedAt,
                c.PublishedAt,
                c.AiGenerationCostUsd,
                c.AiImageCostUsd,
                c.ImageUrl,
                c.ImagePrompt,
                c.ImageUrls,
                c.VideoUrl,
                c.ProjectId
            })
            .ToListAsync(cancellationToken);

        return rows.Select(r => new ContentDto
        {
            Id = r.Id,
            AgencyId = r.AgencyId,
            ContentType = r.ContentType,
            Title = r.Title,
            Body = r.Body,
            Status = r.Status,
            QualityScore = r.QualityScore,
            RelevanceScore = r.RelevanceScore,
            SeoScore = r.SeoScore,
            BrandVoiceScore = r.BrandVoiceScore,
            OverallScore = r.OverallScore,
            ScoreExplanation = r.ScoreExplanation,
            AutoApproved = r.AutoApproved,
            CreatedAt = r.CreatedAt,
            ApprovedAt = r.ApprovedAt,
            PublishedAt = r.PublishedAt,
            AiGenerationCostUsd = r.AiGenerationCostUsd,
            AiImageCostUsd = r.AiImageCostUsd,
            ImageUrl = r.ImageUrl,
            ImagePrompt = r.ImagePrompt,
            ImageUrls = DeserializeImages(r.ImageUrls),
            VideoUrl = r.VideoUrl,
            ProjectId = r.ProjectId
        }).ToList();
    }

    private static List<string>? DeserializeImages(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try { return JsonSerializer.Deserialize<List<string>>(json); }
        catch { return null; }
    }
}
