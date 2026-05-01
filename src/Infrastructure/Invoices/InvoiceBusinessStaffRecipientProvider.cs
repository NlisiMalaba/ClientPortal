using Application.Invoices.Abstractions;
using Microsoft.Extensions.Options;

namespace Infrastructure.Invoices;

public sealed class InvoiceBusinessStaffRecipientProvider : IInvoiceBusinessStaffRecipientProvider
{
    private readonly InvoicePaymentNotificationOptions _options;

    public InvoiceBusinessStaffRecipientProvider(IOptions<InvoicePaymentNotificationOptions> options)
    {
        _options = options.Value;
    }

    public IReadOnlyList<string> GetReceiptConfirmationEmailRecipients()
    {
        return (_options.BusinessStaffEmailRecipients ?? [])
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Select(static value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
