using FluentValidation;

namespace Application.Invoices;

public sealed class RecordPaymentCommandValidator : AbstractValidator<RecordPaymentCommand>
{
    public RecordPaymentCommandValidator()
    {
        RuleFor(command => command.InvoiceId)
            .NotEmpty();

        RuleFor(command => command.ClientId)
            .NotEmpty();

        RuleFor(command => command.Amount)
            .GreaterThan(0m);

        RuleFor(command => command.Currency)
            .NotEmpty()
            .Length(3)
            .Matches("^[A-Za-z]{3}$");

        RuleFor(command => command.Method)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(command => command.Reference)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(command => command.PaidAtUtc)
            .NotEqual(default(DateTime));
    }
}
