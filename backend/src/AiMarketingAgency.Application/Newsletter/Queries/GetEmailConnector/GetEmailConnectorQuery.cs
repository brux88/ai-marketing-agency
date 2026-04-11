using AiMarketingAgency.Application.Newsletter.Dtos;
using MediatR;

namespace AiMarketingAgency.Application.Newsletter.Queries.GetEmailConnector;

public record GetEmailConnectorQuery(Guid AgencyId) : IRequest<EmailConnectorDto?>;
