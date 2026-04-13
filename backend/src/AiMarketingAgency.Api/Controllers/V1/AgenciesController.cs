using AiMarketingAgency.Application.Agencies.Commands.CreateAgency;
using AiMarketingAgency.Application.Agencies.Commands.UpdateApprovalMode;
using AiMarketingAgency.Application.Agencies.Commands.UpdateBrandVoice;
using AiMarketingAgency.Application.Agencies.Commands.UpdateDefaultLlm;
using AiMarketingAgency.Application.Agencies.Commands.UpdateImageSettings;
using AiMarketingAgency.Application.Agencies.Commands.UpdateTargetAudience;
using AiMarketingAgency.Application.Agencies.Dtos;
using AiMarketingAgency.Application.Agencies.Queries.GetAgencies;
using AiMarketingAgency.Application.Agencies.Queries.GetAgencyById;
using AiMarketingAgency.Application.Common.Models;
using AiMarketingAgency.Domain.Enums;
using AiMarketingAgency.Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiMarketingAgency.Api.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class AgenciesController : ControllerBase
{
    private readonly IMediator _mediator;

    public AgenciesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<AgencyDto>>>> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAgenciesQuery(), ct);
        return Ok(ApiResponse<List<AgencyDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<AgencyDto>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAgencyByIdQuery(id), ct);
        if (result == null) return NotFound();
        return Ok(ApiResponse<AgencyDto>.Ok(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<AgencyDto>>> Create([FromBody] CreateAgencyCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<AgencyDto>.Ok(result));
    }

    [HttpPut("{id:guid}/brand-voice")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateBrandVoice(Guid id, [FromBody] UpdateBrandVoiceRequest request, CancellationToken ct)
    {
        await _mediator.Send(new UpdateBrandVoiceCommand(id, request.BrandVoice), ct);
        return Ok(ApiResponse<object>.Ok(null));
    }

    [HttpPut("{id:guid}/target-audience")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateTargetAudience(Guid id, [FromBody] UpdateTargetAudienceRequest request, CancellationToken ct)
    {
        await _mediator.Send(new UpdateTargetAudienceCommand(id, request.TargetAudience), ct);
        return Ok(ApiResponse<object>.Ok(null));
    }

    [HttpPut("{id:guid}/approval-mode")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateApprovalMode(Guid id, [FromBody] UpdateApprovalModeRequest request, CancellationToken ct)
    {
        await _mediator.Send(new UpdateApprovalModeCommand(id, request.ApprovalMode, request.AutoApproveMinScore), ct);
        return Ok(ApiResponse<object>.Ok(null));
    }

    [HttpPut("{id:guid}/default-llm")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateDefaultLlm(Guid id, [FromBody] UpdateDefaultLlmRequest request, CancellationToken ct)
    {
        await _mediator.Send(new UpdateDefaultLlmCommand(id, request.DefaultLlmProviderKeyId, request.ImageLlmProviderKeyId), ct);
        return Ok(ApiResponse<object>.Ok(null));
    }

    [HttpPut("{id:guid}/image-settings")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateImageSettings(Guid id, [FromBody] UpdateImageSettingsRequest request, CancellationToken ct)
    {
        await _mediator.Send(new UpdateImageSettingsCommand(id, request.EnableLogoOverlay, request.LogoOverlayPosition, request.LogoUrl), ct);
        return Ok(ApiResponse<object>.Ok(null));
    }
}

public record UpdateBrandVoiceRequest(BrandVoice BrandVoice);
public record UpdateTargetAudienceRequest(TargetAudience TargetAudience);
public record UpdateApprovalModeRequest(ApprovalMode ApprovalMode, int AutoApproveMinScore);
public record UpdateDefaultLlmRequest(Guid? DefaultLlmProviderKeyId, Guid? ImageLlmProviderKeyId);
public record UpdateImageSettingsRequest(bool EnableLogoOverlay, int LogoOverlayPosition, string? LogoUrl);
