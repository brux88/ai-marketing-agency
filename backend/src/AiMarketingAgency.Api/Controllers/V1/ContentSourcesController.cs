using AiMarketingAgency.Application.Common.Models;
using AiMarketingAgency.Application.ContentSources.Commands.CreateContentSource;
using AiMarketingAgency.Application.ContentSources.Commands.DeleteContentSource;
using AiMarketingAgency.Application.ContentSources.Dtos;
using AiMarketingAgency.Application.ContentSources.Queries.GetContentSourcesByAgency;
using AiMarketingAgency.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiMarketingAgency.Api.Controllers.V1;

[ApiController]
[Route("api/v1/agencies/{agencyId:guid}/sources")]
[Authorize]
public class ContentSourcesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ContentSourcesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ContentSourceDto>>>> GetAll(
        Guid agencyId,
        [FromQuery] Guid? projectId,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetContentSourcesByAgencyQuery(agencyId, projectId), ct);
        return Ok(ApiResponse<List<ContentSourceDto>>.Ok(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ContentSourceDto>>> Create(Guid agencyId, [FromBody] CreateContentSourceRequest request, CancellationToken ct)
    {
        var command = new CreateContentSourceCommand
        {
            AgencyId = agencyId,
            ProjectId = request.ProjectId,
            Type = request.Type,
            Url = request.Url,
            Name = request.Name,
            Config = request.Config,
        };

        var result = await _mediator.Send(command, ct);
        return Ok(ApiResponse<ContentSourceDto>.Ok(result));
    }

    [HttpDelete("{sourceId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid agencyId, Guid sourceId, CancellationToken ct)
    {
        await _mediator.Send(new DeleteContentSourceCommand(sourceId), ct);
        return Ok(ApiResponse<object>.Ok(null));
    }
}

public record CreateContentSourceRequest(
    ContentSourceType Type,
    string Url,
    string? Name = null,
    string? Config = null,
    Guid? ProjectId = null);
