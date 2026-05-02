using Application.Abstractions;
using Application.Auth.Abstractions;
using Domain;
using Infrastructure.Persistence.Public;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infrastructure.Persistence;

public sealed class BusinessRegistrationService : IBusinessRegistrationService
{
    private static readonly JsonSerializerOptions TenantSettingsJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly PublicDbContext _publicDbContext;
    private readonly ITenantProvisioner _tenantProvisioner;
    private readonly string _postgresConnectionString;
    private readonly ILogger<BusinessRegistrationService> _logger;

    public BusinessRegistrationService(
        PublicDbContext publicDbContext,
        ITenantProvisioner tenantProvisioner,
        IConfiguration configuration,
        ILogger<BusinessRegistrationService> logger)
    {
        _publicDbContext = publicDbContext;
        _tenantProvisioner = tenantProvisioner;
        _logger = logger;
        _postgresConnectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("ConnectionStrings:Postgres must be configured.");
    }

    public Task<bool> IsTenantSlugTakenAsync(string slug, CancellationToken cancellationToken = default)
    {
        string normalized = slug.Trim().ToLowerInvariant();
        return _publicDbContext.Tenants.AnyAsync(tenant => tenant.Slug == normalized, cancellationToken);
    }

    public Task<bool> IsTenantDomainTakenAsync(string domain, CancellationToken cancellationToken = default)
    {
        string normalized = domain.Trim().ToLowerInvariant();
        if (!Uri.CheckHostName(normalized).Equals(UriHostNameType.Dns))
        {
            return Task.FromResult(false);
        }

        return _publicDbContext.Tenants.AnyAsync(tenant => tenant.Domain == normalized, cancellationToken);
    }

    public async Task RegisterAsync(Tenant tenant, User ownerUser, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tenant);
        ArgumentNullException.ThrowIfNull(ownerUser);

        string settingsJson = JsonSerializer.Serialize(tenant.Settings, TenantSettingsJsonOptions);

        PublicTenant row = new()
        {
            Id = tenant.Id,
            Slug = tenant.Slug,
            Name = tenant.Name,
            Domain = tenant.Domain,
            Plan = tenant.Plan.ToString(),
            SettingsJson = settingsJson,
            IsActive = tenant.IsActive,
            CreatedAt = DateTime.UtcNow,
        };

        bool publicTenantCommitted = false;
        try
        {
            _publicDbContext.Tenants.Add(row);
            await _publicDbContext.SaveChangesAsync(cancellationToken);
            publicTenantCommitted = true;

            await _tenantProvisioner.CreateSchemaAsync(tenant.Slug, cancellationToken);

            ICurrentTenant provisioningTenant = new SlugCurrentTenant(tenant.Slug);
            await using TenantDbContext tenantDbContext = new(_postgresConnectionString, provisioningTenant);
            tenantDbContext.Set<User>().Add(ownerUser);
            await tenantDbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Registered business tenant {TenantId} slug {TenantSlug} with owner {OwnerId}.",
                tenant.Id,
                tenant.Slug,
                ownerUser.Id);
        }
        catch (Exception)
        {
            if (publicTenantCommitted)
            {
                await TryRevertRegistrationArtifactsAsync(tenant.Id, tenant.Slug, cancellationToken);
            }

            throw;
        }
    }

    private async Task TryRevertRegistrationArtifactsAsync(
        Guid tenantId,
        string slug,
        CancellationToken cancellationToken)
    {
        try
        {
            await _tenantProvisioner.DropTenantSchemaAsync(slug, cancellationToken);

            PublicTenant? existing = await _publicDbContext.Tenants
                .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);

            if (existing is not null)
            {
                _publicDbContext.Tenants.Remove(existing);
                await _publicDbContext.SaveChangesAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to revert registration artifacts for tenant {TenantId} slug {TenantSlug}. Manual cleanup may be required.",
                tenantId,
                slug);
        }
    }

}
