using FluentValidation;

namespace Application.Invoices;

public sealed class ConvertQuoteToInvoiceCommandValidator : AbstractValidator<ConvertQuoteToInvoiceCommand>
{
    public ConvertQuoteToInvoiceCommandValidator()
    {
        RuleFor(command => command.QuoteId)
            .NotEmpty();

        RuleFor(command => command.ClientId)
            .NotEmpty();

        RuleFor(command => command.InvoiceNumber)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(command => command.DueDate)
            .NotEqual(default(DateOnly));
    }
}
