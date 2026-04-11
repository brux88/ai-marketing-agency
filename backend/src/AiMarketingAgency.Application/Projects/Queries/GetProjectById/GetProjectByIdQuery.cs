using AiMarketingAgency.Application.Projects.Dtos;
using MediatR;

namespace AiMarketingAgency.Application.Projects.Queries.GetProjectById;

public record GetProjectByIdQuery(Guid AgencyId, Guid ProjectId) : IRequest<ProjectDto?>;
