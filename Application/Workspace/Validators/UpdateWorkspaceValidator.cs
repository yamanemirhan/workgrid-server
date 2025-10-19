using FluentValidation;
using Application.Workspace.Commands;

namespace Application.Workspace.Validators;

public class UpdateWorkspaceValidator : AbstractValidator<UpdateWorkspaceCommand>
{
    public UpdateWorkspaceValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Workspace ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Workspace name is required")
            .MinimumLength(2).WithMessage("Workspace name must be at least 2 characters")
            .MaximumLength(100).WithMessage("Workspace name cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.Logo)
            .MaximumLength(500).WithMessage("Logo URL cannot exceed 500 characters")
            .Must(BeValidUrl).WithMessage("Logo must be a valid URL")
            .When(x => !string.IsNullOrEmpty(x.Logo));
    }

    private bool BeValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return true;

        return Uri.TryCreate(url, UriKind.Absolute, out _)
            || Uri.TryCreate(url, UriKind.Relative, out _);
    }
}