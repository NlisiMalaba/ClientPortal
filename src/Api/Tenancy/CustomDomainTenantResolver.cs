namespace Api.Tenancy;

public sealed class CustomDomainTenantResolver : ITenantResolver
{
    private readonly ITenantDomainLookup _tenantDomainLookup;

    public CustomDomainTenantResolver(ITenantDomainLookup tenantDomainLookup)
    {
        _tenantDomainLookup = tenantDomainLookup;
    }

    public ValueTask<TenantId?> ResolveAsync(HttpContext httpContext, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        string? host = httpContext.Request.Host.Host?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(host))
        {
            return ValueTask.FromResult<TenantId?>(null);
        }

        return _tenantDomainLookup.FindByDomainAsync(host, cancellationToken);
    }
}
