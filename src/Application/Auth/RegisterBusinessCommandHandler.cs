using Application.Auth.Abstractions;
using Application.Auth.Dtos;
using Domain;
using MediatR;
using Shared;

namespace Application.Auth;

public sealed class RegisterBusinessCommandHandler : IRequestHandler<RegisterBusinessCommand, Result<RegisterBusinessResultDto>>
{
    private static readonly Error TenantSlugTakenError = new(
        "Auth.TenantSlugTaken",
        "Tenant slug is already in use.",
        ErrorType.Conflict);

    private static readonly Error TenantDomainTakenError = new(
        "Auth.TenantDomainTaken",
        "Tenant domain is already in use.",
        ErrorType.Conflict);

    private readonly IBusinessRegistrationService _businessRegistrationService;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterBusinessCommandHandler(
        IBusinessRegistrationService businessRegistrationService,
        IPasswordHasher passwordHasher)
    {
        _businessRegistrationService = businessRegistrationService;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<RegisterBusinessResultDto>> Handle(
        RegisterBusinessCommand request,
        CancellationToken cancellationToken)
    {
        if (await _businessRegistrationService.IsTenantSlugTakenAsync(request.TenantSlug, cancellationToken))
        {
            return Result<RegisterBusinessResultDto>.Failure(TenantSlugTakenError);
        }

        if (await _businessRegistrationService.IsTenantDomainTakenAsync(request.CompanyDomain, cancellationToken))
        {
            return Result<RegisterBusinessResultDto>.Failure(TenantDomainTakenError);
        }

        Guid tenantId = Guid.CreateVersion7();
        Tenant tenant = Tenant.Create(
            id: tenantId,
            slug: request.TenantSlug,
            name: request.CompanyName,
            domain: request.CompanyDomain,
            plan: request.Plan,
            settings: TenantSettings.Default(),
            isActive: true);

        Guid ownerUserId = Guid.CreateVersion7();
        User ownerUser = User.Create(
            id: ownerUserId,
            email: new EmailAddress(request.OwnerEmail),
            fullName: request.OwnerFullName,
            passwordHash: _passwordHasher.Hash(request.OwnerPassword),
            role: Role.Owner,
            isActive: true);

        await _businessRegistrationService.RegisterAsync(tenant, ownerUser, cancellationToken);

        RegisterBusinessResultDto result = new(
            TenantId: tenant.Id,
            OwnerUserId: ownerUser.Id,
            TenantSlug: tenant.Slug);

        return Result<RegisterBusinessResultDto>.Success(result);
    }
}
