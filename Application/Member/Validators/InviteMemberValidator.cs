using FluentValidation;
using Application.Member.Commands;

namespace Application.Member.Validators;

public class InviteMemberValidator : AbstractValidator<InviteMemberCommand>
{
    public InviteMemberValidator()
    {
        RuleFor(x => x.WorkspaceId)
            .NotEmpty()
            .WithMessage("Workspace ID is required");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Invalid email format")
            .MaximumLength(255)
            .WithMessage("Email cannot exceed 255 characters");

        RuleFor(x => x.Role)
            .IsInEnum()
            .WithMessage("Invalid role specified")
            .NotEqual(Domain.Enums.WorkspaceRole.Owner)
            .WithMessage("Cannot invite user as owner");
    }
}