using MediatR;

namespace AiMarketingAgency.Application.Projects.Commands.DeleteProject;

public record DeleteProjectCommand(Guid AgencyId, Guid ProjectId) : IRequest;
