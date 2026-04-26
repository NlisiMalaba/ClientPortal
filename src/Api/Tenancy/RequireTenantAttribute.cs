using System.Reflection;
using Microsoft.AspNetCore.Http.Metadata;

namespace Api.Tenancy;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class RequireTenantAttribute : Attribute, IEndpointMetadataProvider
{
    public static void PopulateMetadata(MethodInfo method, EndpointBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Metadata.Add(new RequireTenantAttribute());
    }
}
