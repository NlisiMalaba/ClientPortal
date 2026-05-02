using Application.Invoices.Abstractions;
using Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Infrastructure.Invoices;

public sealed class NpgsqlRecurringInvoiceGenerator : IRecurringInvoiceGenerator
{
    private readonly string _postgresConnectionString;
    private readonly ILogger<NpgsqlRecurringInvoiceGenerator> _logger;

    public NpgsqlRecurringInvoiceGenerator(
        IConfiguration configuration,
        ILogger<NpgsqlRecurringInvoiceGenerator> logger)
    {
        _postgresConnectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("ConnectionStrings:Postgres must be configured.");
        _logger = logger;
    }

    public async Task<int> GenerateDailyAsync(DateOnly runDate, CancellationToken cancellationToken)
    {
        int generatedCount = 0;

        await using NpgsqlConnection connection = new(_postgresConnectionString);
        await connection.OpenAsync(cancellationToken);

        IReadOnlyList<string> tenantSlugs = await GetActiveTenantSlugsAsync(connection, cancellationToken);
        foreach (string tenantSlug in tenantSlugs)
        {
            string schema = BuildTenantSchemaName(tenantSlug);

            try
            {
                generatedCount += await GenerateForTenantAsync(connection, schema, runDate, cancellationToken);
            }
            catch (PostgresException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Recurring invoice generation skipped for tenant {TenantSlug} due to schema query failure.",
                    tenantSlug);
            }
        }

        return generatedCount;
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

    private static async Task<int> GenerateForTenantAsync(
        NpgsqlConnection connection,
        string schema,
        DateOnly runDate,
        CancellationToken cancellationToken)
    {
        string periodKey = $"{runDate:yyyy-MM}";
        DateOnly dueDate = runDate.AddDays(7);
        string notes = $"Auto-generated recurring invoice for {periodKey}";

        string sql = $"""
            with candidate_projects as (
                select
                    p.client_id,
                    p.id as project_id,
                    p.name as project_name,
                    p.budget,
                    p.currency
                from {schema}.projects p
                inner join {schema}.clients c on c.id = p.client_id
                where p.status = 2
                  and c.status = 2
                  and p.budget > 0
            ),
            projects_without_invoice as (
                select cp.*
                from candidate_projects cp
                where not exists (
                    select 1
                    from {schema}.invoices i
                    where i.project_id = cp.project_id
                      and i.notes = @notes
                )
            ),
            inserted_invoices as (
                insert into {schema}.invoices (
                    id,
                    client_id,
                    project_id,
                    invoice_number,
                    status,
                    subtotal,
                    tax_amount,
                    total,
                    amount_paid,
                    currency,
                    due_date,
                    paid_at,
                    notes,
                    created_at,
                    updated_at
                )
                select
                    gen_random_uuid(),
                    pwi.client_id,
                    pwi.project_id,
                    format('REC-%s-%s', @period_key, upper(substr(replace(pwi.project_id::text, '-', ''), 1, 8))),
                    1,
                    pwi.budget,
                    0,
                    pwi.budget,
                    0,
                    pwi.currency,
                    @due_date,
                    null,
                    @notes,
                    timezone('utc', now()),
                    timezone('utc', now())
                from projects_without_invoice pwi
                returning id, project_id, project_name, budget
            )
            insert into {schema}.line_item (
                invoice_id,
                description,
                quantity,
                unit_price,
                tax_rate,
                amount
            )
            select
                ii.id,
                format('Recurring retainer - %s (%s)', ii.project_name, @period_key),
                1,
                ii.budget,
                0,
                ii.budget
            from inserted_invoices ii;
            """;

        await using NpgsqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("period_key", periodKey);
        command.Parameters.AddWithValue("notes", notes);
        command.Parameters.AddWithValue("due_date", dueDate);
        int lineItemsInserted = await command.ExecuteNonQueryAsync(cancellationToken);
        return lineItemsInserted;
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
