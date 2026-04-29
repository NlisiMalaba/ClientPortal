namespace Application.Clients.Abstractions;

public sealed record ClientInvitationTokenRecord(
    Guid ClientId,
    Guid UserId,
    string TokenHash,
    DateTime ExpiresAtUtc);
