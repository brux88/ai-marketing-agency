using AiMarketingAgency.Application.ContentSources.Dtos;
using AiMarketingAgency.Domain.Enums;
using MediatR;

namespace AiMarketingAgency.Application.ContentSources.Commands.CreateContentSource;

public record CreateContentSourceCommand : IRequest<ContentSourceDto>
{
    public Guid AgencyId { get; init; }
    public Guid? ProjectId { get; init; }
    public ContentSourceType Type { get; init; }
    public string Url { get; init; } = string.Empty;
    public string? Name { get; init; }
    public string? Config { get; init; }
}
