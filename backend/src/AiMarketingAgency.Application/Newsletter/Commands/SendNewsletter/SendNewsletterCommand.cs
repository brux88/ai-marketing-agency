using AiMarketingAgency.Application.Common.Interfaces;
using MediatR;

namespace AiMarketingAgency.Application.Newsletter.Commands.SendNewsletter;

public record SendNewsletterCommand(Guid AgencyId, Guid ContentId) : IRequest<EmailSendResult>;
