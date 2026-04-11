using AiMarketingAgency.Application.Schedules.Dtos;
using MediatR;

namespace AiMarketingAgency.Application.Schedules.Queries.GetSchedulesByAgency;

public record GetSchedulesByAgencyQuery(Guid AgencyId) : IRequest<List<ContentScheduleDto>>;
