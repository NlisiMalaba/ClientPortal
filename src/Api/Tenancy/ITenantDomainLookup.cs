namespace Api.Tenancy;

public interface ITenantDomainLookup
{
    ValueTask<TenantId?> FindByDomainAsync(string domain, CancellationToken cancellationToken = default);
}
