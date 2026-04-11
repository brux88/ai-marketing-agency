using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Application.Schedules.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Application.Schedules.Queries.GetSchedulesByAgency;

public class GetSchedulesByAgencyQueryHandler : IRequestHandler<GetSchedulesByAgencyQuery, List<ContentScheduleDto>>
{
    private readonly IAppDbContext _context;

    public GetSchedulesByAgencyQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ContentScheduleDto>> Handle(GetSchedulesByAgencyQuery request, CancellationToken cancellationToken)
    {
        return await _context.ContentSchedules
            .Where(s => s.AgencyId == request.AgencyId)
            .Include(s => s.Project)
            .Select(s => new ContentScheduleDto
            {
                Id = s.Id,
                AgencyId = s.AgencyId,
                ProjectId = s.ProjectId,
                ProjectName = s.Project != null ? s.Project.Name : null,
                Name = s.Name,
                Days = s.Days,
                TimeOfDay = s.TimeOfDay.ToString("HH:mm"),
                TimeZone = s.TimeZone,
                AgentType = s.AgentType,
                Input = s.Input,
                IsActive = s.IsActive,
                LastRunAt = s.LastRunAt,
                NextRunAt = s.NextRunAt,
                CreatedAt = s.CreatedAt
            })
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }
}
