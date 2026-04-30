using FluentValidation;

namespace Application.Invoices;

public sealed class ProcessGatewayPaymentCommandValidator : AbstractValidator<ProcessGatewayPaymentCommand>
{
    public ProcessGatewayPaymentCommandValidator()
    {
        RuleFor(command => command.Provider)
            .NotEmpty()
            .MaximumLength(64);

        RuleFor(command => command.Payload)
            .NotEmpty();

        RuleFor(command => command.Signature)
            .NotEmpty()
            .MaximumLength(512);
    }
}
