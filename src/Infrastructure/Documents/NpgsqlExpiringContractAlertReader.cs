using Application.Documents.Abstractions;
using Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Infrastructure.Documents;

public sealed class NpgsqlExpiringContractAlertReader : IExpiringContractAlertReader
{
    private static readonly int[] ReminderDays = [30, 7, 1];

    private readonly string _postgresConnectionString;
    private readonly ILogger<NpgsqlExpiringContractAlertReader> _logger;

    public NpgsqlExpiringContractAlertReader(
        IConfiguration configuration,
        ILogger<NpgsqlExpiringContractAlertReader> logger)
    {
        _postgresConnectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("ConnectionStrings:Postgres must be configured.");
        _logger = logger;
    }

    public async Task<IReadOnlyList<ExpiringContractAlertItem>> GetExpiringContractsAsync(
        DateOnly asOfDate,
        CancellationToken cancellationToken)
    {
        List<ExpiringContractAlertItem> alerts = [];

        await using NpgsqlConnection connection = new(_postgresConnectionString);
        await connection.OpenAsync(cancellationToken);

        IReadOnlyList<string> tenantSlugs = await GetActiveTenantSlugsAsync(connection, cancellationToken);
        foreach (string tenantSlug in tenantSlugs)
        {
            string schema = BuildTenantSchemaName(tenantSlug);
            try
            {
                alerts.AddRange(await GetExpiringForTenantAsync(connection, schema, tenantSlug, asOfDate, cancellationToken));
            }
            catch (PostgresException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Skipping expiring contract query for tenant {TenantSlug} because schema query failed.",
                    tenantSlug);
            }
        }

        return alerts;
    }

    private static async Task<IReadOnlyList<string>> GetActiveTenantSlugsAsync(
        NpgsqlConnection connection,
        CancellationToken cancellationToken)
    {
        List<string> tenantSlugs = [];
        await using NpgsqlCommand command = new(PublicTenantActiveSlugsSql.SelectActiveTenantSlugs, connection);
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            tenantSlugs.Add(reader.GetString(0));
        }

        return tenantSlugs;
    }

    private static async Task<IReadOnlyList<ExpiringContractAlertItem>> GetExpiringForTenantAsync(
        NpgsqlConnection connection,
        string schema,
        string tenantSlug,
        DateOnly asOfDate,
        CancellationToken cancellationToken)
    {
        string sql = $"""
            select
                c.id,
                c.client_id,
                c.title,
                c.expires_at,
                cl.company_name,
                cl.email
            from {schema}.contracts c
            inner join {schema}.clients cl on cl.id = c.client_id
            where c.expires_at is not null
              and c.status in (2, 3)
              and c.expires_at >= @window_start
              and c.expires_at < @window_end;
            """;

        DateTime windowStart = asOfDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        DateTime windowEnd = asOfDate.AddDays(31).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        List<ExpiringContractAlertItem> alerts = [];
        await using NpgsqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("window_start", windowStart);
        command.Parameters.AddWithValue("window_end", windowEnd);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            DateTime expiresAtUtc = reader.GetDateTime(3);
            int daysUntilExpiry = (int)Math.Floor((expiresAtUtc.Date - windowStart.Date).TotalDays);
            if (!ReminderDays.Contains(daysUntilExpiry))
            {
                continue;
            }

            alerts.Add(new ExpiringContractAlertItem(
                tenantSlug,
                reader.GetGuid(0),
                reader.GetGuid(1),
                reader.GetString(2),
                expiresAtUtc,
                daysUntilExpiry,
                reader.GetString(4),
                reader.GetString(5)));
        }

        return alerts;
    }

    private static string BuildTenantSchemaName(string tenantSlug)
    {
        string normalizedSlug = tenantSlug.Trim().ToLowerInvariant().Replace("-", "_", StringComparison.Ordinal);
        if (normalizedSlug.Length == 0 || normalizedSlug.Any(ch => !(char.IsAsciiLetterOrDigit(ch) || ch == '_')))
        {
            throw new InvalidOperationException("Tenant slug contains unsupported characters for schema name.");
        }

        return $"tenant_{normalizedSlug}";
    }
}
