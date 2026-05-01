namespace Application.Clients.Abstractions;

public interface IClientInvitationLinkFactory
{
    string CreateAcceptInvitationLink(string inviteToken);
}
