using Application.Invoices.Abstractions;
using Application.Invoices.Dtos;
using Domain;
using Microsoft.EntityFrameworkCore;
using Shared;

namespace Infrastructure.Persistence;

public sealed class InvoiceRepository : IInvoiceRepository
{
    private readonly TenantDbContext _tenantDbContext;

    public InvoiceRepository(TenantDbContext tenantDbContext)
    {
        _tenantDbContext = tenantDbContext;
    }

    public async Task<PagedResult<InvoiceListItemDto>> GetPagedAsync(
        int page,
        int pageSize,
        InvoiceStatus? status,
        Guid? clientId,
        DateOnly? dueDateFrom,
        DateOnly? dueDateTo,
        CancellationToken cancellationToken)
    {
        IQueryable<Invoice> query = _tenantDbContext.Set<Invoice>().AsNoTracking();
        query = ApplyFilters(query, status, clientId, dueDateFrom, dueDateTo);

        int totalCount = await query.CountAsync(cancellationToken);

        IReadOnlyList<InvoiceListItemDto> items = await query
            .OrderByDescending(invoice => invoice.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(invoice => new InvoiceListItemDto(
                invoice.Id,
                invoice.ClientId,
                invoice.ProjectId,
                invoice.InvoiceNumber,
                invoice.Status,
                invoice.Total,
                invoice.AmountPaid,
                decimal.Round(Math.Max(0m, invoice.Total - invoice.AmountPaid), 2, MidpointRounding.ToEven),
                invoice.Currency,
                invoice.DueDate,
                invoice.CreatedAt,
                invoice.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<InvoiceListItemDto>(items, totalCount, page, pageSize);
    }

    public async Task<InvoiceAgingSummaryDto> GetAgingSummaryAsync(
        InvoiceStatus? status,
        Guid? clientId,
        DateOnly? dueDateFrom,
        DateOnly? dueDateTo,
        DateOnly asOfDate,
        CancellationToken cancellationToken)
    {
        IQueryable<Invoice> query = _tenantDbContext.Set<Invoice>().AsNoTracking();
        query = ApplyFilters(query, status, clientId, dueDateFrom, dueDateTo);

        List<Invoice> invoices = await query.ToListAsync(cancellationToken);

        decimal current = 0m;
        decimal days1To30 = 0m;
        decimal days31To60 = 0m;
        decimal days61To90 = 0m;
        decimal days91Plus = 0m;
        int overdueCount = 0;

        foreach (Invoice invoice in invoices)
        {
            if (invoice.Status == InvoiceStatus.Cancelled)
            {
                continue;
            }

            decimal outstanding = decimal.Round(Math.Max(0m, invoice.Total - invoice.AmountPaid), 2, MidpointRounding.ToEven);
            if (outstanding <= 0m)
            {
                continue;
            }

            int overdueDays = asOfDate.DayNumber - invoice.DueDate.DayNumber;
            if (overdueDays <= 0)
            {
                current += outstanding;
                continue;
            }

            overdueCount++;
            if (overdueDays <= 30)
            {
                days1To30 += outstanding;
            }
            else if (overdueDays <= 60)
            {
                days31To60 += outstanding;
            }
            else if (overdueDays <= 90)
            {
                days61To90 += outstanding;
            }
            else
            {
                days91Plus += outstanding;
            }
        }

        decimal totalOutstanding = decimal.Round(
            current + days1To30 + days31To60 + days61To90 + days91Plus,
            2,
            MidpointRounding.ToEven);

        return new InvoiceAgingSummaryDto(
            decimal.Round(current, 2, MidpointRounding.ToEven),
            decimal.Round(days1To30, 2, MidpointRounding.ToEven),
            decimal.Round(days31To60, 2, MidpointRounding.ToEven),
            decimal.Round(days61To90, 2, MidpointRounding.ToEven),
            decimal.Round(days91Plus, 2, MidpointRounding.ToEven),
            totalOutstanding,
            overdueCount);
    }

    public async Task<GetFinancialSummaryResultDto> GetFinancialSummaryAsync(
        Guid? clientId,
        DateOnly? fromDate,
        DateOnly? toDate,
        DateOnly asOfDate,
        CancellationToken cancellationToken)
    {
        IQueryable<Invoice> query = _tenantDbContext.Set<Invoice>().AsNoTracking();

        if (clientId.HasValue)
        {
            query = query.Where(invoice => invoice.ClientId == clientId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(invoice => invoice.DueDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(invoice => invoice.DueDate <= toDate.Value);
        }

        List<Invoice> invoices = await query.ToListAsync(cancellationToken);

        decimal totalOutstanding = 0m;
        int overdueCount = 0;
        foreach (Invoice invoice in invoices)
        {
            if (invoice.Status == InvoiceStatus.Cancelled)
            {
                continue;
            }

            decimal outstanding = decimal.Round(Math.Max(0m, invoice.Total - invoice.AmountPaid), 2, MidpointRounding.ToEven);
            totalOutstanding += outstanding;

            if (outstanding > 0m && invoice.DueDate < asOfDate)
            {
                overdueCount++;
            }
        }

        DateTime monthStartUtc = new(asOfDate.Year, asOfDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTime monthEndUtcExclusive = monthStartUtc.AddMonths(1);

        decimal paidThisMonth = invoices
            .Where(invoice => invoice.PaidAt.HasValue)
            .Where(invoice => invoice.PaidAt!.Value >= monthStartUtc && invoice.PaidAt!.Value < monthEndUtcExclusive)
            .Sum(invoice => invoice.AmountPaid);

        DateOnly windowStart = fromDate ?? asOfDate.AddMonths(-5);
        DateOnly windowEnd = toDate ?? asOfDate;
        if (windowEnd < windowStart)
        {
            windowEnd = windowStart;
        }

        List<CashflowPointDto> cashflow = [];
        DateOnly cursor = new(windowStart.Year, windowStart.Month, 1);
        DateOnly endMonth = new(windowEnd.Year, windowEnd.Month, 1);

        while (cursor <= endMonth)
        {
            DateTime periodStartUtc = new(cursor.Year, cursor.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime periodEndUtcExclusive = periodStartUtc.AddMonths(1);

            decimal inflow = invoices
                .Where(invoice => invoice.PaidAt.HasValue)
                .Where(invoice => invoice.PaidAt!.Value >= periodStartUtc && invoice.PaidAt!.Value < periodEndUtcExclusive)
                .Sum(invoice => invoice.AmountPaid);

            decimal outflow = 0m;
            cashflow.Add(new CashflowPointDto(
                cursor,
                decimal.Round(inflow, 2, MidpointRounding.ToEven),
                outflow,
                decimal.Round(inflow - outflow, 2, MidpointRounding.ToEven)));

            cursor = cursor.AddMonths(1);
        }

        return new GetFinancialSummaryResultDto(
            decimal.Round(totalOutstanding, 2, MidpointRounding.ToEven),
            decimal.Round(paidThisMonth, 2, MidpointRounding.ToEven),
            overdueCount,
            cashflow);
    }

    public Task<Invoice?> FindByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _tenantDbContext.Set<Invoice>().SingleOrDefaultAsync(invoice => invoice.Id == id, cancellationToken);
    }

    public void Add(Invoice invoice)
    {
        _tenantDbContext.Set<Invoice>().Add(invoice);
    }

    public void Update(Invoice invoice)
    {
        _tenantDbContext.Set<Invoice>().Update(invoice);
    }

    public void Delete(Invoice invoice)
    {
        _tenantDbContext.Set<Invoice>().Remove(invoice);
    }

    private static IQueryable<Invoice> ApplyFilters(
        IQueryable<Invoice> query,
        InvoiceStatus? status,
        Guid? clientId,
        DateOnly? dueDateFrom,
        DateOnly? dueDateTo)
    {
        if (status.HasValue)
        {
            query = query.Where(invoice => invoice.Status == status.Value);
        }

        if (clientId.HasValue)
        {
            query = query.Where(invoice => invoice.ClientId == clientId.Value);
        }

        if (dueDateFrom.HasValue)
        {
            query = query.Where(invoice => invoice.DueDate >= dueDateFrom.Value);
        }

        if (dueDateTo.HasValue)
        {
            query = query.Where(invoice => invoice.DueDate <= dueDateTo.Value);
        }

        return query;
    }
}
