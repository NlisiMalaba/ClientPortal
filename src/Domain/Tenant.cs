using Shared;

namespace Domain;

public sealed class Tenant : AggregateRoot<Guid>
{
    public string Slug { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string Domain { get; private set; } = string.Empty;

    public Plan Plan { get; private set; }

    public TenantSettings Settings { get; private set; } = TenantSettings.Default();

    public bool IsActive { get; private set; } = true;

    private Tenant()
    {
    }

    private Tenant(
        Guid id,
        string slug,
        string name,
        string domain,
        Plan plan,
        TenantSettings settings,
        bool isActive)
        : base(id)
    {
        Slug = NormalizeSlug(slug);
        Name = NormalizeName(name);
        Domain = NormalizeDomain(domain);
        Plan = plan;
        Settings = Guard.NotNull(settings, nameof(settings));
        IsActive = isActive;
    }

    public static Tenant Create(
        Guid id,
        string slug,
        string name,
        string domain,
        Plan plan,
        TenantSettings settings,
        bool isActive = true)
    {
        return new Tenant(id, slug, name, domain, plan, settings, isActive);
    }

    public void UpdateName(string name)
    {
        Name = NormalizeName(name);
        MarkUpdated();
    }

    public void UpdateDomain(string domain)
    {
        Domain = NormalizeDomain(domain);
        MarkUpdated();
    }

    public void ChangePlan(Plan plan)
    {
        Plan = plan;
        MarkUpdated();
    }

    public void UpdateSettings(TenantSettings settings)
    {
        Settings = Guard.NotNull(settings, nameof(settings));
        MarkUpdated();
    }

    public void Activate()
    {
        IsActive = true;
        MarkUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkUpdated();
    }

    private static string NormalizeSlug(string slug)
    {
        string normalized = Guard.NotEmpty(slug, nameof(slug)).Trim().ToLowerInvariant();
        if (normalized.Any(ch => !(char.IsAsciiLetterOrDigit(ch) || ch == '-')))
        {
            throw new ArgumentException("Slug must contain only lowercase letters, digits, and hyphens.", nameof(slug));
        }

        return normalized;
    }

    private static string NormalizeName(string name)
    {
        return Guard.NotEmpty(name, nameof(name)).Trim();
    }

    private static string NormalizeDomain(string domain)
    {
        string normalized = Guard.NotEmpty(domain, nameof(domain)).Trim().ToLowerInvariant();

        if (!Uri.CheckHostName(normalized).Equals(UriHostNameType.Dns))
        {
            throw new ArgumentException("Domain must be a valid DNS host name.", nameof(domain));
        }

        return normalized;
    }
}
