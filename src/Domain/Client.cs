using Shared;

namespace Domain;

public sealed class Client : AggregateRoot<Guid>
{
    public string CompanyName { get; private set; } = string.Empty;

    public string ContactName { get; private set; } = string.Empty;

    public EmailAddress Email { get; private set; } = null!;

    public PhoneNumber Phone { get; private set; } = null!;

    public ClientStatus Status { get; private set; } = ClientStatus.Invited;

    public DateTime InvitedAt { get; private set; }

    public DateTime? OnboardedAt { get; private set; }

    public string? Notes { get; private set; }

    private Client()
    {
    }

    private Client(
        Guid id,
        string companyName,
        string contactName,
        EmailAddress email,
        PhoneNumber phone,
        ClientStatus status,
        DateTime invitedAt,
        DateTime? onboardedAt,
        string? notes)
        : base(id)
    {
        CompanyName = NormalizeRequiredText(companyName, nameof(companyName));
        ContactName = NormalizeRequiredText(contactName, nameof(contactName));
        Email = Guard.NotNull(email, nameof(email));
        Phone = Guard.NotNull(phone, nameof(phone));
        Status = status;
        InvitedAt = NormalizeUtc(invitedAt);
        OnboardedAt = NormalizeOptionalUtc(onboardedAt);
        Notes = NormalizeNotes(notes);

        EnsureStatusConsistency(Status, OnboardedAt);
    }

    public static Client Invite(
        Guid id,
        string companyName,
        string contactName,
        EmailAddress email,
        PhoneNumber phone,
        string? notes = null,
        DateTime? invitedAtUtc = null)
    {
        DateTime invitedAt = NormalizeUtc(invitedAtUtc ?? DateTime.UtcNow);
        Client client = new(
            id,
            companyName,
            contactName,
            email,
            phone,
            ClientStatus.Invited,
            invitedAt,
            null,
            notes);

        client.AddDomainEvent(new ClientInvitedEvent(client.Id, invitedAt));
        return client;
    }

    public static Client Create(
        Guid id,
        string companyName,
        string contactName,
        EmailAddress email,
        PhoneNumber phone,
        ClientStatus status,
        DateTime invitedAtUtc,
        DateTime? onboardedAtUtc = null,
        string? notes = null)
    {
        return new Client(id, companyName, contactName, email, phone, status, invitedAtUtc, onboardedAtUtc, notes);
    }

    public void UpdateProfile(string companyName, string contactName, EmailAddress email, PhoneNumber phone)
    {
        CompanyName = NormalizeRequiredText(companyName, nameof(companyName));
        ContactName = NormalizeRequiredText(contactName, nameof(contactName));
        Email = Guard.NotNull(email, nameof(email));
        Phone = Guard.NotNull(phone, nameof(phone));
        MarkUpdated();
    }

    public void UpdateNotes(string? notes)
    {
        Notes = NormalizeNotes(notes);
        MarkUpdated();
    }

    public void Onboard(DateTime? onboardedAtUtc = null)
    {
        DateTime timestamp = NormalizeUtc(onboardedAtUtc ?? DateTime.UtcNow);
        if (timestamp < InvitedAt)
        {
            throw new ArgumentException("Onboarding timestamp cannot be before the invitation timestamp.", nameof(onboardedAtUtc));
        }

        Status = ClientStatus.Active;
        OnboardedAt = timestamp;
        MarkUpdated();
        AddDomainEvent(new ClientOnboardedEvent(Id, timestamp));
    }

    public void Suspend()
    {
        if (Status == ClientStatus.Suspended)
        {
            return;
        }

        Status = ClientStatus.Suspended;
        MarkUpdated();
    }

    public void Activate()
    {
        if (Status == ClientStatus.Active)
        {
            return;
        }

        Status = ClientStatus.Active;
        if (!OnboardedAt.HasValue)
        {
            OnboardedAt = DateTime.UtcNow;
        }

        MarkUpdated();
    }

    public void Deactivate(DateTime? deactivatedAtUtc = null)
    {
        DateTime timestamp = NormalizeUtc(deactivatedAtUtc ?? DateTime.UtcNow);
        if (Status == ClientStatus.Inactive)
        {
            return;
        }

        Status = ClientStatus.Inactive;
        MarkUpdated();
        AddDomainEvent(new ClientDeactivatedEvent(Id, timestamp));
    }

    private static void EnsureStatusConsistency(ClientStatus status, DateTime? onboardedAt)
    {
        if (status == ClientStatus.Invited && onboardedAt.HasValue)
        {
            throw new ArgumentException("Invited clients cannot have an onboarding timestamp.", nameof(onboardedAt));
        }

        if (status is ClientStatus.Active or ClientStatus.Suspended or ClientStatus.Inactive && !onboardedAt.HasValue)
        {
            throw new ArgumentException(
                "Active, suspended, and inactive clients must have an onboarding timestamp.",
                nameof(onboardedAt));
        }
    }

    private static string NormalizeRequiredText(string value, string paramName)
    {
        return Guard.NotEmpty(value, paramName).Trim();
    }

    private static string? NormalizeNotes(string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
        {
            return null;
        }

        return notes.Trim();
    }

    private static DateTime NormalizeUtc(DateTime value)
    {
        DateTime normalized = value;
        if (normalized.Kind == DateTimeKind.Local)
        {
            normalized = normalized.ToUniversalTime();
        }
        else if (normalized.Kind == DateTimeKind.Unspecified)
        {
            normalized = DateTime.SpecifyKind(normalized, DateTimeKind.Utc);
        }

        return normalized;
    }

    private static DateTime? NormalizeOptionalUtc(DateTime? value)
    {
        if (!value.HasValue)
        {
            return null;
        }

        return NormalizeUtc(value.Value);
    }
}
