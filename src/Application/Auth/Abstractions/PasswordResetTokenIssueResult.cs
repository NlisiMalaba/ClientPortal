namespace Application.Auth.Abstractions;

public sealed record PasswordResetTokenIssueResult(
    string Token,
    string TokenHash,
    DateTime ExpiresAtUtc);
