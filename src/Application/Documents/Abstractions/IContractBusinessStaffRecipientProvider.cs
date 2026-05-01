namespace Application.Documents.Abstractions;

public interface IContractBusinessStaffRecipientProvider
{
    IReadOnlyList<string> GetSignedContractNotificationRecipients();

    IReadOnlyList<string> GetExpiringContractNotificationRecipients();
}
