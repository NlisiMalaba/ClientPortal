namespace Api.Tenancy;

public readonly record struct TenantId(string Value)
{
    public override string ToString()
    {
        return Value;
    }
}

public interface ITenantResolver
{
    ValueTask<TenantId?> ResolveAsync(HttpContext httpContext, CancellationToken cancellationToken = default);
}
