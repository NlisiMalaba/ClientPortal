using Application.Documents.Abstractions;
using Microsoft.Extensions.Options;

namespace Infrastructure.Documents;

public sealed class ContractBusinessStaffRecipientProvider : IContractBusinessStaffRecipientProvider
{
    private readonly ContractNotificationOptions _options;

    public ContractBusinessStaffRecipientProvider(IOptions<ContractNotificationOptions> options)
    {
        _options = options.Value;
    }

    public IReadOnlyList<string> GetSignedContractNotificationRecipients()
    {
        return (_options.SignedBusinessStaffEmailRecipients ?? [])
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Select(static value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public IReadOnlyList<string> GetExpiringContractNotificationRecipients()
    {
        return (_options.ExpiringBusinessStaffEmailRecipients ?? [])
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Select(static value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
