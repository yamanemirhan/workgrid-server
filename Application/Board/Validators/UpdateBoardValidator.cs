using FluentValidation;
using Application.Board.Commands;

namespace Application.Board.Validators;

public class UpdateBoardValidator : AbstractValidator<UpdateBoardCommand>
{
    public UpdateBoardValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Board ID is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Board title is required")
            .MinimumLength(2).WithMessage("Board title must be at least 2 characters")
            .MaximumLength(100).WithMessage("Board title cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

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