using FluentValidation;
using Application.Board.Commands;

namespace Application.Board.Validators;

public class CreateBoardValidator : AbstractValidator<CreateBoardCommand>
{
    public CreateBoardValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Board title is required")
            .MinimumLength(2).WithMessage("Board title must be at least 2 characters")
            .MaximumLength(100).WithMessage("Board title cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

        RuleFor(x => x.WorkspaceId)
            .NotEmpty().WithMessage("Workspace ID is required");
    }
}