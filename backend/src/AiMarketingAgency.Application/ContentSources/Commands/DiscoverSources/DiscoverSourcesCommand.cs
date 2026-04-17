using MediatR;

namespace AiMarketingAgency.Application.ContentSources.Commands.DiscoverSources;

public record DiscoverSourcesCommand(Guid AgencyId, Guid? ProjectId) : IRequest<List<SuggestedSource>>;

public record SuggestedSource(string Url, string Name, string Description, int Type);
