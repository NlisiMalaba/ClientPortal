namespace Infrastructure.Persistence;

/// <summary>
/// Raw SQL for public.tenants must match <see cref="PublicDbContext"/> column names (PascalCase), not tenant-schema snake_case conventions.
/// </summary>
internal static class PublicTenantActiveSlugsSql
{
    internal const string SelectActiveTenantSlugs = """
        select "Slug"
        from public.tenants
        where "IsActive" = true;
        """;
}
