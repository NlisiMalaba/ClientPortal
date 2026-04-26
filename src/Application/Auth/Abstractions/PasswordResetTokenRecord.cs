namespace Application.Auth.Abstractions;

public sealed record PasswordResetTokenRecord(
    Guid UserId,
    string TokenHash,
    DateTime ExpiresAtUtc);
