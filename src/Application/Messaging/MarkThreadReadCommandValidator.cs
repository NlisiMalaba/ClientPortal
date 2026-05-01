using FluentValidation;

namespace Application.Messaging;

public sealed class MarkThreadReadCommandValidator : AbstractValidator<MarkThreadReadCommand>
{
    public MarkThreadReadCommandValidator()
    {
        RuleFor(command => command.ThreadId)
            .NotEmpty();

        RuleFor(command => command.ReaderId)
            .NotEmpty();
    }
}
