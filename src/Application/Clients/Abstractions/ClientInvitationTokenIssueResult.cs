namespace Application.Clients.Abstractions;

public sealed record ClientInvitationTokenIssueResult(
    string Token,
    string TokenHash,
    DateTime ExpiresAtUtc);
