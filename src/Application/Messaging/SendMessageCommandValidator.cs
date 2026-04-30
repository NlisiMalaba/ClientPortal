using FluentValidation;

namespace Application.Messaging;

public sealed class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(command => command.ThreadId)
            .NotEmpty();

        RuleFor(command => command.SenderId)
            .NotEmpty();

        RuleFor(command => command.SenderRole)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.ClientMessageId)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(command => command.Content)
            .NotEmpty()
            .MaximumLength(4000);
    }
}
