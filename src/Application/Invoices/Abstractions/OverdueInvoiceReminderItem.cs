namespace Application.Invoices.Abstractions;

public sealed record OverdueInvoiceReminderItem(
    string TenantSlug,
    Guid InvoiceId,
    string InvoiceNumber,
    DateOnly DueDate,
    decimal OutstandingAmount,
    string Currency,
    string ClientContactName,
    string ClientEmail,
    string ClientPhone);
