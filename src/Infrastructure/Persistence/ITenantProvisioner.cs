namespace Infrastructure.Persistence;

public interface ITenantProvisioner
{
    Task CreateSchemaAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Drops the tenant-specific PostgreSQL schema (if present). Used to compensate failed registration.
    /// </summary>
    Task DropTenantSchemaAsync(string slug, CancellationToken cancellationToken = default);
}
