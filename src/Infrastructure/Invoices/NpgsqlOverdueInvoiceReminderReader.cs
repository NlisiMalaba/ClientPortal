using Application.Invoices.Abstractions;
using Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Infrastructure.Invoices;

public sealed class NpgsqlOverdueInvoiceReminderReader : IOverdueInvoiceReminderReader
{
    private readonly string _postgresConnectionString;
    private readonly ILogger<NpgsqlOverdueInvoiceReminderReader> _logger;

    public NpgsqlOverdueInvoiceReminderReader(
        IConfiguration configuration,
        ILogger<NpgsqlOverdueInvoiceReminderReader> logger)
    {
        _postgresConnectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("ConnectionStrings:Postgres must be configured.");
        _logger = logger;
    }

    public async Task<IReadOnlyList<OverdueInvoiceReminderItem>> GetOverdueInvoicesAsync(
        DateOnly asOfDate,
        CancellationToken cancellationToken)
    {
        List<OverdueInvoiceReminderItem> reminders = [];

        await using NpgsqlConnection connection = new(_postgresConnectionString);
        await connection.OpenAsync(cancellationToken);

        List<string> tenantSlugs = await GetActiveTenantSlugsAsync(connection, cancellationToken);
        foreach (string tenantSlug in tenantSlugs)
        {
            string schema = BuildTenantSchemaName(tenantSlug);
            IReadOnlyList<OverdueInvoiceReminderItem> tenantReminders = await GetOverdueForTenantAsync(
                connection,
                schema,
                tenantSlug,
                asOfDate,
                cancellationToken);
            reminders.AddRange(tenantReminders);
        }

        return reminders;
    }

    private static async Task<List<string>> GetActiveTenantSlugsAsync(
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

    private async Task<IReadOnlyList<OverdueInvoiceReminderItem>> GetOverdueForTenantAsync(
        NpgsqlConnection connection,
        string schema,
        string tenantSlug,
        DateOnly asOfDate,
        CancellationToken cancellationToken)
    {
        List<OverdueInvoiceReminderItem> reminders = [];
        string sql = $"""
            select
                i.id,
                i.invoice_number,
                i.due_date,
                greatest(i.total - coalesce(i.amount_paid, 0), 0) as outstanding_amount,
                i.currency,
                c.contact_name,
                c.email,
                c.phone
            from {schema}.invoices i
            inner join {schema}.clients c on c.id = i.client_id
            where i.due_date < @as_of_date
              and i.status in (2, 3, 4, 6)
              and (i.total - coalesce(i.amount_paid, 0)) > 0;
            """;

        await using NpgsqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("as_of_date", asOfDate);

        try
        {
            await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                reminders.Add(new OverdueInvoiceReminderItem(
                    tenantSlug,
                    reader.GetGuid(0),
                    reader.GetString(1),
                    reader.GetFieldValue<DateOnly>(2),
                    reader.GetDecimal(3),
                    reader.GetString(4),
                    reader.GetString(5),
                    reader.GetString(6),
                    reader.GetString(7)));
            }
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(
                ex,
                "Skipping overdue reminder scan for tenant {TenantSlug} because schema query failed.",
                tenantSlug);
        }

        return reminders;
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
