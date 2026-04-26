using Domain;
using Shared;

namespace Application.Auth.Abstractions;

public interface IJwtTokenService
{
    AccessTokenIssueResult GenerateAccessToken(User user, Tenant tenant);

    Result<JwtTokenValidationResult> ValidateToken(string token);
}
