using Domain;

namespace Application.Auth.Abstractions;

public interface IBusinessRegistrationService
{
    Task<bool> IsTenantSlugTakenAsync(string slug, CancellationToken cancellationToken = default);

    Task<bool> IsTenantDomainTakenAsync(string domain, CancellationToken cancellationToken = default);

    Task RegisterAsync(Tenant tenant, User ownerUser, CancellationToken cancellationToken = default);
}
