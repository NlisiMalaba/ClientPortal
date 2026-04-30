using FluentValidation;

namespace Application.Invoices;

public sealed class UpdateInvoiceCommandValidator : AbstractValidator<UpdateInvoiceCommand>
{
    public UpdateInvoiceCommandValidator()
    {
        RuleFor(command => command.InvoiceId)
            .NotEmpty();

        RuleFor(command => command.ClientId)
            .NotEmpty();

        RuleFor(command => command.InvoiceNumber)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(command => command.Currency)
            .NotEmpty()
            .Length(3)
            .Matches("^[A-Za-z]{3}$");

        RuleFor(command => command.DueDate)
            .NotEqual(default(DateOnly));

        RuleFor(command => command.LineItems)
            .NotNull()
            .Must(items => items.Count > 0)
            .WithMessage("At least one line item is required.");

        RuleForEach(command => command.LineItems)
            .SetValidator(new UpdateInvoiceLineItemInputValidator());
    }

    private sealed class UpdateInvoiceLineItemInputValidator : AbstractValidator<CreateInvoiceLineItemInput>
    {
        public UpdateInvoiceLineItemInputValidator()
        {
            RuleFor(item => item.Description)
                .NotEmpty()
                .MaximumLength(512);

            RuleFor(item => item.Quantity)
                .GreaterThan(0m);

            RuleFor(item => item.UnitPrice)
                .GreaterThanOrEqualTo(0m);

            RuleFor(item => item.TaxRate)
                .InclusiveBetween(0m, 1m);
        }
    }
}
