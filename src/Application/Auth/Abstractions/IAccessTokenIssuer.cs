using Domain;

namespace Application.Auth.Abstractions;

public interface IAccessTokenIssuer
{
    AccessTokenIssueResult Issue(User user);
}
