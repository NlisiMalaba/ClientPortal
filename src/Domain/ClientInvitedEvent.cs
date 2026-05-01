namespace Domain;

public sealed record ClientInvitedEvent(
    Guid ClientId,
    Guid ClientUserId,
    string RecipientEmail,
    string ContactName,
    string InviteToken,
    DateTime InvitedAt) : IDomainEvent;
