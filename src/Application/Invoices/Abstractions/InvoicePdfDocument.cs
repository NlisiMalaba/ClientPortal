namespace Application.Invoices.Abstractions;

public sealed record InvoicePdfDocument(
    string FileName,
    string ContentType,
    byte[] Content);
