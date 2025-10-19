using FluentValidation;
using Application.Card.Commands;

namespace Application.Card.Validators;

public class CreateCardValidator : AbstractValidator<CreateCardCommand>
{
    public CreateCardValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Card title is required")
            .MinimumLength(1).WithMessage("Card title must be at least 1 character")
            .MaximumLength(200).WithMessage("Card title cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");

        RuleFor(x => x.ListId)
            .NotEmpty().WithMessage("List ID is required");

        RuleFor(x => x.EstimatedTime)
            .MaximumLength(50).WithMessage("Estimated time cannot exceed 50 characters")
            .Matches(@"^(\d+\s*(min|minute|minutes|hour|hours|day|days|week|weeks|month|months|h|d|w|m))?$")
            .WithMessage("Estimated time must be in valid format (e.g., '10min', '2 hours', '3 days')")
            .When(x => !string.IsNullOrEmpty(x.EstimatedTime));

        RuleFor(x => x.Tags)
            .MaximumLength(500).WithMessage("Tags cannot exceed 500 characters");
    }
}

public class UpdateCardValidator : AbstractValidator<UpdateCardCommand>
{
    public UpdateCardValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Card ID is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Card title is required")
            .MinimumLength(1).WithMessage("Card title must be at least 1 character")
            .MaximumLength(200).WithMessage("Card title cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");

        RuleFor(x => x.EstimatedTime)
            .MaximumLength(50).WithMessage("Estimated time cannot exceed 50 characters")
            .Matches(@"^(\d+\s*(min|minute|minutes|hour|hours|day|days|week|weeks|month|months|h|d|w|m))?$")
            .WithMessage("Estimated time must be in valid format (e.g., '10min', '2 hours', '3 days')")
            .When(x => !string.IsNullOrEmpty(x.EstimatedTime));

        RuleFor(x => x.Tags)
            .MaximumLength(500).WithMessage("Tags cannot exceed 500 characters");
    }
}

public class DeleteCardValidator : AbstractValidator<DeleteCardCommand>
{
    public DeleteCardValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Card ID is required");
    }
}