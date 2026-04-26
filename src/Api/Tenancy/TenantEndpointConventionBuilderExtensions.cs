using Microsoft.AspNetCore.Builder;

namespace Api.Tenancy;

public static class TenantEndpointConventionBuilderExtensions
{
    public static TBuilder RequireTenant<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.WithMetadata(new RequireTenantAttribute());
        return builder;
    }
}
