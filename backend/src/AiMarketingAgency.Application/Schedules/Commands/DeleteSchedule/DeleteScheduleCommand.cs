using MediatR;

namespace AiMarketingAgency.Application.Schedules.Commands.DeleteSchedule;

public record DeleteScheduleCommand(Guid Id, Guid AgencyId) : IRequest;
