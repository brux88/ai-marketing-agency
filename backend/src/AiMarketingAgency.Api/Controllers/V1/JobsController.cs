using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Application.Common.Models;
using AiMarketingAgency.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Api.Controllers.V1;

[ApiController]
[Route("api/v1/jobs")]
[Authorize]
public class JobsController : ControllerBase
{
    private readonly IAppDbContext _context;

    public JobsController(IAppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<JobListItemDto>>>> List(
        [FromQuery] Guid? agencyId,
        [FromQuery] int take = 50,
        CancellationToken ct = default)
    {
        var query = _context.AgentJobs.AsQueryable();
        if (agencyId.HasValue) query = query.Where(j => j.AgencyId == agencyId.Value);

        var items = await query
            .OrderByDescending(j => j.CreatedAt)
            .Take(Math.Clamp(take, 1, 200))
            .Select(j => new JobListItemDto
            {
                Id = j.Id,
                AgencyId = j.AgencyId,
                ProjectId = j.ProjectId,
                AgentType = j.AgentType.ToString(),
                Status = j.Status.ToString(),
                Input = j.Input,
                Output = j.Output,
                ErrorMessage = j.ErrorMessage,
                CreatedAt = j.CreatedAt,
                StartedAt = j.StartedAt,
                CompletedAt = j.CompletedAt
            })
            .ToListAsync(ct);

        return Ok(ApiResponse<List<JobListItemDto>>.Ok(items));
    }
}

public class JobListItemDto
{
    public Guid Id { get; set; }
    public Guid AgencyId { get; set; }
    public Guid? ProjectId { get; set; }
    public string AgentType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Input { get; set; }
    public string? Output { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
