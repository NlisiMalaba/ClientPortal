namespace Application.Documents.Abstractions;

public sealed record ExpiringContractAlertItem(
    string TenantSlug,
    Guid ContractId,
    Guid ClientId,
    string ContractTitle,
    DateTime ExpiresAtUtc,
    int DaysUntilExpiry,
    string ClientCompanyName,
    string ClientEmail);
