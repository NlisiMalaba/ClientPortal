using System.Globalization;
using Shared;

namespace Domain;

public sealed class Project : AggregateRoot<Guid>
{
    public Guid ClientId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public ProjectStatus Status { get; private set; } = ProjectStatus.Planned;

    public DateOnly StartDate { get; private set; }

    public DateOnly EndDate { get; private set; }

    public decimal Budget { get; private set; }

    public string Currency { get; private set; } = string.Empty;

    private Project()
    {
    }

    private Project(
        Guid id,
        Guid clientId,
        string name,
        string description,
        ProjectStatus status,
        DateOnly startDate,
        DateOnly endDate,
        decimal budget,
        string currency)
        : base(id)
    {
        ClientId = NormalizeClientId(clientId);
        Name = NormalizeName(name);
        Description = NormalizeDescription(description);
        Status = status;
        (StartDate, EndDate) = NormalizeDateRange(startDate, endDate);
        Budget = NormalizeBudget(budget);
        Currency = NormalizeCurrency(currency);
    }

    public static Project Create(
        Guid id,
        Guid clientId,
        string name,
        string description,
        ProjectStatus status,
        DateOnly startDate,
        DateOnly endDate,
        decimal budget,
        string currency)
    {
        Project project = new(id, clientId, name, description, status, startDate, endDate, budget, currency);
        project.AddDomainEvent(new ProjectCreatedEvent(project.Id, project.ClientId, DateTime.UtcNow));
        return project;
    }

    public void UpdateName(string name)
    {
        Name = NormalizeName(name);
        MarkUpdated();
    }

    public void UpdateDescription(string description)
    {
        Description = NormalizeDescription(description);
        MarkUpdated();
    }

    public void UpdateStatus(ProjectStatus status)
    {
        Status = status;
        MarkUpdated();
    }

    public void UpdateTimeline(DateOnly startDate, DateOnly endDate)
    {
        (StartDate, EndDate) = NormalizeDateRange(startDate, endDate);
        MarkUpdated();
    }

    public void UpdateBudget(decimal budget, string currency)
    {
        Budget = NormalizeBudget(budget);
        Currency = NormalizeCurrency(currency);
        MarkUpdated();
    }

    private static Guid NormalizeClientId(Guid clientId)
    {
        if (clientId == Guid.Empty)
        {
            throw new ArgumentException("ClientId cannot be empty.", nameof(clientId));
        }

        return clientId;
    }

    private static string NormalizeName(string name)
    {
        return Guard.NotEmpty(name, nameof(name)).Trim();
    }

    private static string NormalizeDescription(string description)
    {
        return Guard.NotEmpty(description, nameof(description)).Trim();
    }

    private static (DateOnly StartDate, DateOnly EndDate) NormalizeDateRange(DateOnly startDate, DateOnly endDate)
    {
        if (endDate < startDate)
        {
            throw new ArgumentException("EndDate cannot be earlier than StartDate.", nameof(endDate));
        }

        return (startDate, endDate);
    }

    private static decimal NormalizeBudget(decimal budget)
    {
        if (budget < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(budget), "Budget cannot be negative.");
        }

        return decimal.Round(budget, 2, MidpointRounding.ToEven);
    }

    private static string NormalizeCurrency(string currency)
    {
        string normalizedCurrency = Guard.NotEmpty(currency, nameof(currency)).Trim().ToUpper(CultureInfo.InvariantCulture);
        if (normalizedCurrency.Length != 3 || normalizedCurrency.Any(ch => !char.IsAsciiLetter(ch)))
        {
            throw new ArgumentException("Currency must be a 3-letter ISO code.", nameof(currency));
        }

        return normalizedCurrency;
    }
}
