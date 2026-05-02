namespace Infrastructure.Persistence;

/// <summary>
/// Carries the tenant slug discovered during cross-tenant authentication lookups (e.g. login by email).
/// </summary>
public static class TenantResolutionContext
{
    private static readonly AsyncLocal<string?> TenantSlug = new();

    public static void Clear() => TenantSlug.Value = null;

    public static void SetSlug(string slug) => TenantSlug.Value = slug.Trim().ToLowerInvariant();

    public static string? Slug => TenantSlug.Value;
}
