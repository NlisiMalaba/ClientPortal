using FluentValidation;

namespace Application.Messaging;

public sealed class CreateThreadCommandValidator : AbstractValidator<CreateThreadCommand>
{
    public CreateThreadCommandValidator()
    {
        RuleFor(command => command.ClientId)
            .NotEmpty();

        RuleFor(command => command.CreatorId)
            .NotEmpty();

        RuleFor(command => command.ParticipantIds)
            .NotNull()
            .Must(participants => participants.Count > 0)
            .WithMessage("At least one participant is required.");

        RuleForEach(command => command.ParticipantIds)
            .NotEmpty();

        RuleFor(command => command.Subject)
            .NotEmpty()
            .MaximumLength(300);
    }
}
