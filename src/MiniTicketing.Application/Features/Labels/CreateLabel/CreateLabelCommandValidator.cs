namespace MiniTicketing.Application.Features.Labels.CreateLabel;

using FluentValidation;

public sealed class CreateLabelCommandValidator : AbstractValidator<CreateLabelCommand>
{
    public CreateLabelCommandValidator()
    {
        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .Must(name => !string.IsNullOrWhiteSpace(name))
                .WithMessage("Name must not be empty or whitespace.")
            .Must(name => (name?.Trim().Length ?? 0) <= 100)
                .WithMessage("Name must be at most 100 characters.");
    }
}
