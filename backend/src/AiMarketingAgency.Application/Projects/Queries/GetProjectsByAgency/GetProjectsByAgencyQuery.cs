using AiMarketingAgency.Application.Projects.Dtos;
using MediatR;

namespace AiMarketingAgency.Application.Projects.Queries.GetProjectsByAgency;

public record GetProjectsByAgencyQuery(Guid AgencyId) : IRequest<List<ProjectDto>>;
