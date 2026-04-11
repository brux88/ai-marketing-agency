using FluentValidation;

namespace AiMarketingAgency.Application.Agencies.Commands.CreateAgency;

public class CreateAgencyCommandValidator : AbstractValidator<CreateAgencyCommand>
{
    public CreateAgencyCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Agency name is required")
            .MaximumLength(200);

        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(2000);

        RuleFor(x => x.WebsiteUrl)
            .MaximumLength(500);

        RuleFor(x => x.AutoApproveMinScore)
            .InclusiveBetween(1, 10)
            .WithMessage("Score must be between 1 and 10");
    }
}
