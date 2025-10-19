using FluentValidation;
using Application.Member.Commands;

namespace Application.Member.Validators;

public class LeaveWorkspaceValidator : AbstractValidator<LeaveWorkspaceCommand>
{
    public LeaveWorkspaceValidator()
    {
        RuleFor(x => x.WorkspaceId)
            .NotEmpty()
            .WithMessage("Workspace ID is required");
    }
}