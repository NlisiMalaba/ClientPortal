using Application.Clients.Abstractions;
using Microsoft.Extensions.Options;

namespace Infrastructure.Clients;

public sealed class ClientInvitationLinkFactory : IClientInvitationLinkFactory
{
    private readonly ClientInvitationLinkFactoryOptions _options;

    public ClientInvitationLinkFactory(IOptions<ClientInvitationLinkFactoryOptions> options)
    {
        _options = options.Value;
    }

    public string CreateAcceptInvitationLink(string inviteToken)
    {
        string normalizedToken = string.IsNullOrWhiteSpace(inviteToken)
            ? throw new ArgumentException("Invite token cannot be empty.", nameof(inviteToken))
            : inviteToken.Trim();

        string baseUrl = string.IsNullOrWhiteSpace(_options.AcceptInvitationBaseUrl)
            ? throw new InvalidOperationException("ClientInvitations:AcceptInvitationBaseUrl must be configured.")
            : _options.AcceptInvitationBaseUrl.Trim();

        string separator = baseUrl.Contains('?', StringComparison.Ordinal) ? "&" : "?";
        return $"{baseUrl}{separator}token={Uri.EscapeDataString(normalizedToken)}";
    }
}
