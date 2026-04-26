namespace Api.Tenancy;

public sealed class NullTenantDomainLookup : ITenantDomainLookup
{
    public ValueTask<TenantId?> FindByDomainAsync(string domain, CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult<TenantId?>(null);
    }
}
