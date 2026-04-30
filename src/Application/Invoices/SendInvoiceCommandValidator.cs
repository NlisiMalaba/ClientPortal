using FluentValidation;

namespace Application.Invoices;

public sealed class SendInvoiceCommandValidator : AbstractValidator<SendInvoiceCommand>
{
    public SendInvoiceCommandValidator()
    {
        RuleFor(command => command.InvoiceId)
            .NotEmpty();

        RuleFor(command => command.ClientId)
            .NotEmpty();
    }
}
