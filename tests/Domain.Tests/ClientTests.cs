using Domain;

namespace Domain.Tests;

public sealed class ClientTests
{
    [Fact]
    public void Invite_SetsInvitedState_AndRaisesInvitedEvent()
    {
        DateTime invitedAt = new(2026, 04, 29, 10, 0, 0, DateTimeKind.Utc);

        Client client = Client.Invite(
            Guid.NewGuid(),
            "Contoso Ltd",
            "Jane Doe",
            new EmailAddress("jane.doe@contoso.com"),
            new PhoneNumber("+14155552671"),
            invitedAtUtc: invitedAt);

        Assert.Equal(ClientStatus.Invited, client.Status);
        Assert.Equal(invitedAt, client.InvitedAt);
        Assert.Null(client.OnboardedAt);

        ClientInvitedEvent @event = Assert.IsType<ClientInvitedEvent>(Assert.Single(client.DomainEvents));
        Assert.Equal(client.Id, @event.ClientId);
        Assert.Equal(invitedAt, @event.InvitedAt);
    }

    [Fact]
    public void Onboard_ActivatesClient_SetsTimestamp_AndRaisesEvent()
    {
        DateTime invitedAt = new(2026, 04, 29, 10, 0, 0, DateTimeKind.Utc);
        DateTime onboardedAt = new(2026, 04, 29, 11, 0, 0, DateTimeKind.Utc);

        Client client = Client.Invite(
            Guid.NewGuid(),
            "Contoso Ltd",
            "Jane Doe",
            new EmailAddress("jane.doe@contoso.com"),
            new PhoneNumber("+14155552671"),
            invitedAtUtc: invitedAt);

        client.ClearDomainEvents();
        client.Onboard(onboardedAt);

        Assert.Equal(ClientStatus.Active, client.Status);
        Assert.Equal(onboardedAt, client.OnboardedAt);

        ClientOnboardedEvent @event = Assert.IsType<ClientOnboardedEvent>(Assert.Single(client.DomainEvents));
        Assert.Equal(client.Id, @event.ClientId);
        Assert.Equal(onboardedAt, @event.OnboardedAt);
    }

    [Fact]
    public void Deactivate_SetsInactive_AndRaisesDeactivatedEvent()
    {
        DateTime invitedAt = new(2026, 04, 29, 10, 0, 0, DateTimeKind.Utc);
        DateTime onboardedAt = new(2026, 04, 29, 11, 0, 0, DateTimeKind.Utc);
        DateTime deactivatedAt = new(2026, 04, 29, 12, 0, 0, DateTimeKind.Utc);

        Client client = Client.Invite(
            Guid.NewGuid(),
            "Contoso Ltd",
            "Jane Doe",
            new EmailAddress("jane.doe@contoso.com"),
            new PhoneNumber("+14155552671"),
            invitedAtUtc: invitedAt);

        client.Onboard(onboardedAt);
        client.ClearDomainEvents();
        client.Deactivate(deactivatedAt);

        Assert.Equal(ClientStatus.Inactive, client.Status);

        ClientDeactivatedEvent @event = Assert.IsType<ClientDeactivatedEvent>(Assert.Single(client.DomainEvents));
        Assert.Equal(client.Id, @event.ClientId);
        Assert.Equal(deactivatedAt, @event.DeactivatedAt);
    }
}
