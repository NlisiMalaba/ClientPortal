using System.Globalization;
using System.Text;
using Application.Abstractions;
using Application.Invoices.Abstractions;
using Domain;

namespace Infrastructure.Invoices;

public sealed class SimpleInvoicePdfGenerator : IInvoicePdfGenerator
{
    private readonly ICurrentTenant _currentTenant;

    public SimpleInvoicePdfGenerator(ICurrentTenant currentTenant)
    {
        _currentTenant = currentTenant;
    }

    public Task<InvoicePdfDocument> GenerateAsync(Invoice invoice, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        TenantSettings? tenantSettings = _currentTenant.Settings;
        string brandColour = tenantSettings?.BrandColour ?? "#2563EB";
        string logoUrl = tenantSettings?.LogoUrl ?? "N/A";

        string textBody = BuildInvoiceBody(invoice, brandColour, logoUrl);
        byte[] content = BuildMinimalPdf(textBody);

        InvoicePdfDocument document = new(
            FileName: $"{invoice.InvoiceNumber}.pdf",
            ContentType: "application/pdf",
            Content: content);

        return Task.FromResult(document);
    }

    private static string BuildInvoiceBody(Invoice invoice, string brandColour, string logoUrl)
    {
        StringBuilder builder = new();
        builder.AppendLine($"Invoice: {invoice.InvoiceNumber}");
        builder.AppendLine($"ClientId: {invoice.ClientId}");
        builder.AppendLine($"ProjectId: {invoice.ProjectId}");
        builder.AppendLine($"DueDate: {invoice.DueDate:yyyy-MM-dd}");
        builder.AppendLine($"Status: {invoice.Status}");
        builder.AppendLine($"BrandColour: {brandColour}");
        builder.AppendLine($"LogoUrl: {logoUrl}");
        builder.AppendLine();
        builder.AppendLine("Line Items:");

        foreach (LineItem lineItem in invoice.LineItems)
        {
            builder.AppendLine(
                $"- {lineItem.Description} | Qty: {lineItem.Quantity.ToString("0.##", CultureInfo.InvariantCulture)} | " +
                $"Unit: {lineItem.UnitPrice.ToString("0.00", CultureInfo.InvariantCulture)} | " +
                $"Tax: {(lineItem.TaxRate * 100m).ToString("0.##", CultureInfo.InvariantCulture)}% | " +
                $"Amount: {lineItem.Amount.ToString("0.00", CultureInfo.InvariantCulture)}");
        }

        builder.AppendLine();
        builder.AppendLine($"Subtotal: {invoice.Subtotal.ToString("0.00", CultureInfo.InvariantCulture)} {invoice.Currency}");
        builder.AppendLine($"Tax: {invoice.TaxAmount.ToString("0.00", CultureInfo.InvariantCulture)} {invoice.Currency}");
        builder.AppendLine($"Total: {invoice.Total.ToString("0.00", CultureInfo.InvariantCulture)} {invoice.Currency}");
        builder.AppendLine($"AmountPaid: {invoice.AmountPaid.ToString("0.00", CultureInfo.InvariantCulture)} {invoice.Currency}");
        builder.AppendLine($"Outstanding: {(invoice.Total - invoice.AmountPaid).ToString("0.00", CultureInfo.InvariantCulture)} {invoice.Currency}");
        if (!string.IsNullOrWhiteSpace(invoice.Notes))
        {
            builder.AppendLine($"Notes: {invoice.Notes}");
        }

        return builder.ToString();
    }

    // Minimal valid one-page PDF wrapper around text content.
    private static byte[] BuildMinimalPdf(string text)
    {
        string escapedText = text
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("(", "\\(", StringComparison.Ordinal)
            .Replace(")", "\\)", StringComparison.Ordinal)
            .Replace("\r", string.Empty, StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal);

        string pdf = $"""
%PDF-1.4
1 0 obj
<< /Type /Catalog /Pages 2 0 R >>
endobj
2 0 obj
<< /Type /Pages /Kids [3 0 R] /Count 1 >>
endobj
3 0 obj
<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >>
endobj
4 0 obj
<< /Length {escapedText.Length + 36} >>
stream
BT
/F1 10 Tf
50 740 Td
({escapedText}) Tj
ET
endstream
endobj
5 0 obj
<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>
endobj
xref
0 6
0000000000 65535 f 
0000000010 00000 n 
0000000063 00000 n 
0000000122 00000 n 
0000000248 00000 n 
0000000384 00000 n 
trailer
<< /Size 6 /Root 1 0 R >>
startxref
454
%%EOF
""";

        return Encoding.ASCII.GetBytes(pdf);
    }
}
