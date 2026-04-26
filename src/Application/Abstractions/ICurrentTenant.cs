using Domain;

namespace Application.Abstractions;

public interface ICurrentTenant
{
    string? TenantId { get; }

    string? Slug { get; }

    TenantSettings? Settings { get; }

    bool IsResolved { get; }
}
