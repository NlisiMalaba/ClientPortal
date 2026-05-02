using Application.Abstractions;
using Domain;

namespace Infrastructure.Persistence;

/// <summary>
/// Resolves a tenant slug for provisioning or auth lookups without HTTP tenant middleware.
/// </summary>
public sealed class SlugCurrentTenant : ICurrentTenant
{
    public SlugCurrentTenant(string slug)
    {
        Slug = slug.Trim().ToLowerInvariant();
    }

    public string? TenantId => null;

    public string? Slug { get; }

    public TenantSettings? Settings => null;

    public bool IsResolved => !string.IsNullOrWhiteSpace(Slug);
}
