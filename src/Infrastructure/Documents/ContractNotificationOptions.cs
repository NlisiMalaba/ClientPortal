namespace Infrastructure.Documents;

public sealed class ContractNotificationOptions
{
    public const string SectionName = "Contracts:Notifications";

    public string[] SignedBusinessStaffEmailRecipients { get; set; } = [];
}
