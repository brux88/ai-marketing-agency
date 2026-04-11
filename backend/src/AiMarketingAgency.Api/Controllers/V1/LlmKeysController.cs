using AiMarketingAgency.Application.Common.Models;
using AiMarketingAgency.Application.LlmKeys.Commands.AddLlmKey;
using AiMarketingAgency.Application.LlmKeys.Commands.DeleteLlmKey;
using AiMarketingAgency.Application.LlmKeys.Dtos;
using AiMarketingAgency.Application.LlmKeys.Queries.GetLlmKeys;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiMarketingAgency.Api.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class LlmKeysController : ControllerBase
{
    private readonly IMediator _mediator;

    public LlmKeysController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<LlmKeyDto>>>> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetLlmKeysQuery(), ct);
        return Ok(ApiResponse<List<LlmKeyDto>>.Ok(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<LlmKeyDto>>> Add([FromBody] AddLlmKeyCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetAll), ApiResponse<LlmKeyDto>.Ok(result));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteLlmKeyCommand(id), ct);
        return Ok(ApiResponse<object>.Ok(null));
    }
}
