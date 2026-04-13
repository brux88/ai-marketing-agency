using AiMarketingAgency.Application.Projects.Dtos;
using MediatR;

namespace AiMarketingAgency.Application.Projects.Commands.ExtractBrandFromWebsite;

public record ExtractBrandFromWebsiteCommand(Guid AgencyId, Guid ProjectId) : IRequest<ProjectDto>;
