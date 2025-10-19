using FluentValidation;
using Application.List.Commands;

namespace Application.List.Validators;

public class CreateListValidator : AbstractValidator<CreateListCommand>
{
    public CreateListValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("List title is required")
            .MinimumLength(1).WithMessage("List title must be at least 1 character")
            .MaximumLength(100).WithMessage("List title cannot exceed 100 characters");

        RuleFor(x => x.BoardId)
            .NotEmpty().WithMessage("Board ID is required");
    }
}