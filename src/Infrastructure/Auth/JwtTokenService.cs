using Application.Auth.Abstractions;
using Domain;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Infrastructure.Auth;

public sealed class JwtTokenService : IJwtTokenService
{
    private static readonly Error InvalidTokenError = new(
        "Auth.InvalidToken",
        "Token is invalid.",
        ErrorType.Forbidden);

    private static readonly Error ExpiredTokenError = new(
        "Auth.TokenExpired",
        "Token has expired.",
        ErrorType.Forbidden);

    private readonly JwtOptions _options;
    private readonly JwtSecurityTokenHandler _handler = new();
    private readonly RsaSecurityKey _privateSigningKey;
    private readonly RsaSecurityKey _publicValidationKey;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
        ValidateOptions(_options);
        _privateSigningKey = JwtRsaKeyFactory.CreatePrivateKey(_options.PrivateKeyPem);
        _publicValidationKey = JwtRsaKeyFactory.CreatePublicKey(_options.PublicKeyPem);
    }

    public AccessTokenIssueResult GenerateAccessToken(User user, Tenant tenant)
    {
        DateTime expiresAtUtc = DateTime.UtcNow.AddMinutes(_options.AccessTokenLifetimeMinutes);
        SigningCredentials signingCredentials = new(
            _privateSigningKey,
            SecurityAlgorithms.RsaSha256);

        List<Claim> claims =
        [
            new("userId", user.Id.ToString()),
            new("tenantId", tenant.Id.ToString()),
            new("tenantSlug", tenant.Slug),
            new("role", user.Role.ToString())
        ];
        claims.AddRange(user.Permissions.Select(permission => new Claim("permissions", permission.Value)));

        JwtSecurityToken token = new(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAtUtc,
            signingCredentials: signingCredentials);

        string serializedToken = _handler.WriteToken(token);
        return new AccessTokenIssueResult(serializedToken, expiresAtUtc);
    }

    public Result<JwtTokenValidationResult> ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Result<JwtTokenValidationResult>.Failure(InvalidTokenError);
        }

        TokenValidationParameters parameters = CreateValidationParameters(validateLifetime: true);

        try
        {
            ClaimsPrincipal principal = _handler.ValidateToken(token.Trim(), parameters, out SecurityToken validatedToken);
            if (validatedToken is not JwtSecurityToken jwtToken)
            {
                return Result<JwtTokenValidationResult>.Failure(InvalidTokenError);
            }

            return Result<JwtTokenValidationResult>.Success(
                new JwtTokenValidationResult(
                    Token: token.Trim(),
                    Principal: principal,
                    ExpiresAtUtc: jwtToken.ValidTo));
        }
        catch (SecurityTokenExpiredException)
        {
            return Result<JwtTokenValidationResult>.Failure(ExpiredTokenError);
        }
        catch (SecurityTokenException)
        {
            return Result<JwtTokenValidationResult>.Failure(InvalidTokenError);
        }
        catch (ArgumentException)
        {
            return Result<JwtTokenValidationResult>.Failure(InvalidTokenError);
        }
    }

    private TokenValidationParameters CreateValidationParameters(bool validateLifetime)
    {
        return new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _publicValidationKey,
            ValidateIssuer = true,
            ValidIssuer = _options.Issuer,
            ValidateAudience = true,
            ValidAudience = _options.Audience,
            ValidateLifetime = validateLifetime,
            ClockSkew = TimeSpan.Zero
        };
    }

    private static void ValidateOptions(JwtOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Issuer))
        {
            throw new InvalidOperationException("Jwt:Issuer must be configured.");
        }

        if (string.IsNullOrWhiteSpace(options.Audience))
        {
            throw new InvalidOperationException("Jwt:Audience must be configured.");
        }

        if (string.IsNullOrWhiteSpace(options.PrivateKeyPem))
        {
            throw new InvalidOperationException("Jwt:PrivateKeyPem must be configured.");
        }

        if (string.IsNullOrWhiteSpace(options.PublicKeyPem))
        {
            throw new InvalidOperationException("Jwt:PublicKeyPem must be configured.");
        }

        if (options.AccessTokenLifetimeMinutes <= 0)
        {
            throw new InvalidOperationException("Jwt:AccessTokenLifetimeMinutes must be greater than zero.");
        }
    }
}
