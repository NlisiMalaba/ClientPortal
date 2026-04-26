namespace Application.Auth.Abstractions;

public sealed record AccessTokenIssueResult(
    string AccessToken,
    DateTime ExpiresAt);
