using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Application.Content.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Application.Content.Commands.UpdateContent;

public class UpdateContentCommandHandler : IRequestHandler<UpdateContentCommand, ContentDto>
{
    private readonly IAppDbContext _context;

    public UpdateContentCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<ContentDto> Handle(UpdateContentCommand request, CancellationToken cancellationToken)
    {
        var content = await _context.GeneratedContents
            .FirstOrDefaultAsync(c => c.Id == request.ContentId && c.AgencyId == request.AgencyId, cancellationToken)
            ?? throw new KeyNotFoundException("Content not found.");

        content.Title = request.Title;
        content.Body = request.Body;

        await _context.SaveChangesAsync(cancellationToken);

        return new ContentDto
        {
            Id = content.Id,
            AgencyId = content.AgencyId,
            ContentType = content.ContentType,
            Title = content.Title,
            Body = content.Body,
            Status = content.Status,
            QualityScore = content.QualityScore,
            RelevanceScore = content.RelevanceScore,
            SeoScore = content.SeoScore,
            BrandVoiceScore = content.BrandVoiceScore,
            OverallScore = content.OverallScore,
            ScoreExplanation = content.ScoreExplanation,
            AutoApproved = content.AutoApproved,
            CreatedAt = content.CreatedAt,
            ApprovedAt = content.ApprovedAt,
            ImageUrl = content.ImageUrl,
            ImagePrompt = content.ImagePrompt,
            VideoUrl = content.VideoUrl,
        };
    }
}
