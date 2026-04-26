namespace Application.Auth.Abstractions;

public sealed record RefreshTokenIssueResult(
    string Token,
    string TokenHash,
    DateTime ExpiresAt,
    string CreatedByIp);
