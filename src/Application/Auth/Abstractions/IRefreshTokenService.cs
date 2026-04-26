namespace Application.Auth.Abstractions;

public interface IRefreshTokenService
{
    RefreshTokenIssueResult Generate(string createdByIp, DateTime? nowUtc = null);

    string Hash(string refreshToken);

    bool Validate(string refreshToken, string storedTokenHash);
}
