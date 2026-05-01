namespace Infrastructure.Invoices;

public sealed class InvoicePaymentNotificationOptions
{
    public const string SectionName = "Invoices:PaymentNotifications";

    public string[] BusinessStaffEmailRecipients { get; set; } = [];
}
