using AiMarketingAgency.Domain.Enums;
using MediatR;

namespace AiMarketingAgency.Application.Agents.Commands.RunAgent;

public record RunAgentCommand(Guid AgencyId, AgentType AgentType, string? Input = null, Guid? ProjectId = null) : IRequest<Guid>;
