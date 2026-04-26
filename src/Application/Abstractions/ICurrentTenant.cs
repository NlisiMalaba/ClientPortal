namespace Application.Abstractions;

public interface ICurrentTenant
{
    string? TenantId { get; }

    bool IsResolved { get; }
}
