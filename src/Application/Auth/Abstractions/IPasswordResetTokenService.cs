namespace Application.Auth.Abstractions;

public interface IPasswordResetTokenService
{
    PasswordResetTokenIssueResult Issue(DateTime? nowUtc = null);

    string Hash(string token);
}
