namespace Infrastructure.Clients;

public sealed class ClientInvitationLinkFactoryOptions
{
    public const string SectionName = "ClientInvitations";

    public string AcceptInvitationBaseUrl { get; set; } = "https://app.clientportal.local/accept-invitation";
}
