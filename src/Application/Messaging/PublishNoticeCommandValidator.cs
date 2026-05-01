using FluentValidation;

namespace Application.Messaging;

public sealed class PublishNoticeCommandValidator : AbstractValidator<PublishNoticeCommand>
{
    public PublishNoticeCommandValidator()
    {
        RuleFor(command => command.Title)
            .NotEmpty()
            .MaximumLength(250);

        RuleFor(command => command.Content)
            .NotEmpty()
            .MaximumLength(8000);

        RuleFor(command => command.ExpiresAt)
            .Must(value => !value.HasValue || value.Value > DateTime.UtcNow)
            .WithMessage("ExpiresAt must be a future datetime when provided.");

        RuleForEach(command => command.TargetClientIds)
            .NotEmpty();
    }
}
