using FluentValidation;
using Application.List.Commands;

namespace Application.List.Validators;

public class UpdateListPositionValidator : AbstractValidator<UpdateListPositionCommand>
{
    public UpdateListPositionValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("List ID is required");

        RuleFor(x => x.Position)
            .GreaterThanOrEqualTo(0).WithMessage("Position must be 0 or greater");
    }
}
