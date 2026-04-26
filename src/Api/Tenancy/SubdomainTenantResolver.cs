namespace Api.Tenancy;

public sealed class SubdomainTenantResolver : ITenantResolver
{
    private const string DefaultRootDomain = "clientportal.app";

    private readonly string _rootDomain;

    public SubdomainTenantResolver(string? rootDomain = null)
    {
        _rootDomain = string.IsNullOrWhiteSpace(rootDomain)
            ? DefaultRootDomain
            : rootDomain.Trim().ToLowerInvariant();
    }

    public ValueTask<TenantId?> ResolveAsync(HttpContext httpContext, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        string? host = httpContext.Request.Host.Host?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(host))
        {
            return ValueTask.FromResult<TenantId?>(null);
        }

        if (string.Equals(host, _rootDomain, StringComparison.OrdinalIgnoreCase))
        {
            return ValueTask.FromResult<TenantId?>(null);
        }

        string suffix = $".{_rootDomain}";
        if (!host.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
        {
            return ValueTask.FromResult<TenantId?>(null);
        }

        string[] labels = host.Split('.');
        string[] rootLabels = _rootDomain.Split('.');

        // Strictly expect one subdomain label: slug.clientportal.app
        if (labels.Length != rootLabels.Length + 1)
        {
            return ValueTask.FromResult<TenantId?>(null);
        }

        string slug = labels[0];
        if (!IsValidSlug(slug))
        {
            return ValueTask.FromResult<TenantId?>(null);
        }

        return ValueTask.FromResult<TenantId?>(new TenantId(slug));
    }

    private static bool IsValidSlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return false;
        }

        if (slug.StartsWith('-') || slug.EndsWith('-'))
        {
            return false;
        }

        return slug.All(ch => char.IsAsciiLetterOrDigit(ch) || ch == '-');
    }
}
