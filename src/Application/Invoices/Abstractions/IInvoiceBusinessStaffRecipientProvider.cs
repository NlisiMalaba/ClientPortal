namespace Application.Invoices.Abstractions;

public interface IInvoiceBusinessStaffRecipientProvider
{
    IReadOnlyList<string> GetReceiptConfirmationEmailRecipients();
}
