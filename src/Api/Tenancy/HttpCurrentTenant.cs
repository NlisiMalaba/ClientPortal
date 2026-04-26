using Application.Abstractions;

namespace Api.Tenancy;

public sealed class HttpCurrentTenant : ICurrentTenant
{
    private const string TenantHeaderName = "X-Tenant-Id";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCurrentTenant(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? TenantId
    {
        get
        {
            string? tenantId = _httpContextAccessor.HttpContext?.Request.Headers[TenantHeaderName].FirstOrDefault();
            return string.IsNullOrWhiteSpace(tenantId) ? null : tenantId.Trim();
        }
    }

    public bool IsResolved => !string.IsNullOrWhiteSpace(TenantId);
}
