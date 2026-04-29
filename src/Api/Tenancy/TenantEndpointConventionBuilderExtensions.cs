using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authorization;
using Api.Auth;

namespace Api.Tenancy;

public static class TenantEndpointConventionBuilderExtensions
{
    public static TBuilder RequireTenant<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.WithMetadata(new RequireTenantAttribute());
        builder.RequireAuthorization(AuthorizationPolicies.RequireTenantAccess);
        return builder;
    }
}
