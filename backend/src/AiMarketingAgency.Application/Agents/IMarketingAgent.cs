using AiMarketingAgency.Domain.Entities;
using AiMarketingAgency.Domain.Enums;
using Microsoft.SemanticKernel;

namespace AiMarketingAgency.Application.Agents;

public interface IMarketingAgent
{
    AgentType Type { get; }
    Task<AgentJobResult> ExecuteAsync(AgentJobContext context, CancellationToken ct);
}

public record AgentJobContext(
    Kernel Kernel,
    Agency Agency,
    string? Input,
    IReadOnlyList<ContentSource> Sources,
    Project? Project = null,
    ContentSchedule? Schedule = null,
    IReadOnlyList<RecentContentSummary>? RecentContents = null,
    IReadOnlyList<ProjectDocumentSummary>? Documents = null);

public record RecentContentSummary(
    string Title,
    string? BodyExcerpt,
    ContentType ContentType,
    DateTime CreatedAt);

public record ProjectDocumentSummary(
    string Name,
    string ExtractedText);

public record AgentJobResult(
    bool Success,
    string? Output,
    List<GeneratedContentResult> Contents);

public record GeneratedContentResult(
    string Title,
    string Body,
    ContentType Type,
    decimal QualityScore,
    decimal RelevanceScore,
    decimal SeoScore,
    decimal BrandVoiceScore,
    decimal OverallScore,
    string? ScoreExplanation,
    string? ImageUrl = null,
    string? ImagePrompt = null);
