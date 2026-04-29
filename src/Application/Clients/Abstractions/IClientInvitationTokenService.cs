namespace Application.Clients.Abstractions;

public interface IClientInvitationTokenService
{
    ClientInvitationTokenIssueResult Issue(DateTime? nowUtc = null);

    string Hash(string token);
}
