using Application.Auth.Abstractions;
using Application.Abstractions;
using Domain;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Public;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Auth;

public sealed class JwtAccessTokenIssuer : IAccessTokenIssuer
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly PublicDbContext _publicDbContext;
    private readonly ICurrentTenant _currentTenant;
    private readonly string _postgresConnectionString;

    public JwtAccessTokenIssuer(
        IJwtTokenService jwtTokenService,
        PublicDbContext publicDbContext,
        ICurrentTenant currentTenant,
        IConfiguration configuration)
    {
        _jwtTokenService = jwtTokenService;
        _publicDbContext = publicDbContext;
        _currentTenant = currentTenant;
        _postgresConnectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("ConnectionStrings:Postgres must be configured.");
    }

    public AccessTokenIssueResult Issue(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        string? slug = TenantResolutionContext.Slug;
        if (string.IsNullOrWhiteSpace(slug))
        {
            slug = _currentTenant.Slug;
        }

        if (string.IsNullOrWhiteSpace(slug))
        {
            slug = ResolveTenantSlugByUserId(user.Id);
        }

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

    private string? ResolveTenantSlugByUserId(Guid userId)
    {
        List<string> slugs = _publicDbContext.Tenants
            .AsNoTracking()
            .Where(tenant => tenant.IsActive)
            .OrderBy(tenant => tenant.Slug)
            .Select(tenant => tenant.Slug)
            .ToList();

        foreach (string slug in slugs)
        {
            using TenantDbContext tenantDbContext = new(_postgresConnectionString, new SlugCurrentTenant(slug));
            bool exists = tenantDbContext.Set<User>().AsNoTracking().Any(user => user.Id == userId);
            if (exists)
            {
                return slug;
            }
        }

        return null;
    }
}
