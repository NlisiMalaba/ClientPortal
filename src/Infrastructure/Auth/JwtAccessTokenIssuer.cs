using Application.Auth.Abstractions;
using Domain;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Public;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Auth;

public sealed class JwtAccessTokenIssuer : IAccessTokenIssuer
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly PublicDbContext _publicDbContext;

    public JwtAccessTokenIssuer(IJwtTokenService jwtTokenService, PublicDbContext publicDbContext)
    {
        _jwtTokenService = jwtTokenService;
        _publicDbContext = publicDbContext;
    }

    public AccessTokenIssueResult Issue(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        string? slug = TenantResolutionContext.Slug;
        TenantResolutionContext.Clear();
        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new InvalidOperationException(
                "Tenant slug was not resolved. Ensure authentication flows set TenantResolutionContext after user lookup.");
        }

        PublicTenant? row = _publicDbContext.Tenants
            .AsNoTracking()
            .SingleOrDefault(tenant => tenant.Slug == slug);

        if (row is null)
        {
            throw new InvalidOperationException($"Public tenant record for slug '{slug}' was not found.");
        }

        Tenant tenant = PublicTenantMapper.ToDomain(row);
        return _jwtTokenService.GenerateAccessToken(user, tenant);
    }
}
