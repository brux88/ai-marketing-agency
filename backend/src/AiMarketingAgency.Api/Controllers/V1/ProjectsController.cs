using AiMarketingAgency.Application.Common.Models;
using AiMarketingAgency.Application.Projects.Commands.CreateProject;
using AiMarketingAgency.Application.Projects.Commands.DeleteProject;
using AiMarketingAgency.Application.Projects.Commands.UpdateProject;
using AiMarketingAgency.Application.Projects.Dtos;
using AiMarketingAgency.Application.Projects.Queries.GetProjectById;
using AiMarketingAgency.Application.Projects.Queries.GetProjectsByAgency;
using AiMarketingAgency.Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiMarketingAgency.Api.Controllers.V1;

[ApiController]
[Route("api/v1/agencies/{agencyId:guid}/projects")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProjectsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ProjectDto>>>> GetAll(Guid agencyId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProjectsByAgencyQuery(agencyId), ct);
        return Ok(ApiResponse<List<ProjectDto>>.Ok(result));
    }

    [HttpGet("{projectId:guid}")]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> GetById(Guid agencyId, Guid projectId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProjectByIdQuery(agencyId, projectId), ct);
        if (result == null) return NotFound();
        return Ok(ApiResponse<ProjectDto>.Ok(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> Create(Guid agencyId, [FromBody] CreateProjectRequest request, CancellationToken ct)
    {
        var command = new CreateProjectCommand
        {
            AgencyId = agencyId,
            Name = request.Name,
            Description = request.Description,
            WebsiteUrl = request.WebsiteUrl,
            LogoUrl = request.LogoUrl,
            BrandVoice = request.BrandVoice,
            TargetAudience = request.TargetAudience
        };

        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { agencyId, projectId = result.Id }, ApiResponse<ProjectDto>.Ok(result));
    }

    [HttpPut("{projectId:guid}")]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> Update(Guid agencyId, Guid projectId, [FromBody] UpdateProjectRequest request, CancellationToken ct)
    {
        var command = new UpdateProjectCommand
        {
            ProjectId = projectId,
            AgencyId = agencyId,
            Name = request.Name,
            Description = request.Description,
            WebsiteUrl = request.WebsiteUrl,
            LogoUrl = request.LogoUrl,
            BrandVoice = request.BrandVoice,
            TargetAudience = request.TargetAudience
        };

        var result = await _mediator.Send(command, ct);
        return Ok(ApiResponse<ProjectDto>.Ok(result));
    }

    [HttpDelete("{projectId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid agencyId, Guid projectId, CancellationToken ct)
    {
        await _mediator.Send(new DeleteProjectCommand(agencyId, projectId), ct);
        return Ok(ApiResponse<object>.Ok(null));
    }
}

public record CreateProjectRequest(
    string Name,
    string? Description = null,
    string? WebsiteUrl = null,
    string? LogoUrl = null,
    BrandVoice? BrandVoice = null,
    TargetAudience? TargetAudience = null);

public record UpdateProjectRequest(
    string Name,
    string? Description = null,
    string? WebsiteUrl = null,
    string? LogoUrl = null,
    BrandVoice? BrandVoice = null,
    TargetAudience? TargetAudience = null);
