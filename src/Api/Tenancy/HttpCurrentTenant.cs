using Application.Abstractions;
using Domain;

namespace Api.Tenancy;

public sealed class HttpCurrentTenant : ICurrentTenant
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCurrentTenant(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? TenantId
    {
        get
        {
            object? value = _httpContextAccessor.HttpContext?.Items[TenantHttpContextKeys.TenantId];
            string? tenantId = value as string;
            return string.IsNullOrWhiteSpace(tenantId) ? null : tenantId;
        }
    }

    public string? Slug
    {
        get
        {
            object? value = _httpContextAccessor.HttpContext?.Items[TenantHttpContextKeys.TenantSlug];
            string? slug = value as string;
            return string.IsNullOrWhiteSpace(slug) ? null : slug;
        }
    }

    public TenantSettings? Settings
    {
        get
        {
            object? value = _httpContextAccessor.HttpContext?.Items[TenantHttpContextKeys.TenantSettings];
            return value as TenantSettings;
        }
    }

    public bool IsResolved => !string.IsNullOrWhiteSpace(TenantId);
}
