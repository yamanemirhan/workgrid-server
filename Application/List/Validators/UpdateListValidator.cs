using FluentValidation;
using Application.List.Commands;

namespace Application.List.Validators;

public class UpdateListValidator : AbstractValidator<UpdateListCommand>
{
    public UpdateListValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("List ID is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("List title is required")
            .MinimumLength(1).WithMessage("List title must be at least 1 character")
            .MaximumLength(100).WithMessage("List title cannot exceed 100 characters");
    }
}