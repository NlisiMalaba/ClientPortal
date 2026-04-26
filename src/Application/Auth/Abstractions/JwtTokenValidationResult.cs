using System.Security.Claims;

namespace Application.Auth.Abstractions;

public sealed record JwtTokenValidationResult(
    string Token,
    ClaimsPrincipal Principal,
    DateTime ExpiresAtUtc);
