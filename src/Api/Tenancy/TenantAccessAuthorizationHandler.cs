using Microsoft.AspNetCore.Authorization;

namespace Api.Tenancy;

public sealed class TenantAccessAuthorizationHandler : AuthorizationHandler<TenantAccessRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        TenantAccessRequirement requirement)
    {
        string? userTenantId = context.User.FindFirst("tenantId")?.Value;
        if (string.IsNullOrWhiteSpace(userTenantId))
        {
            return Task.CompletedTask;
        }

        if (context.Resource is not HttpContext httpContext)
        {
            return Task.CompletedTask;
        }

        object? resourceTenant = httpContext.Items[TenantHttpContextKeys.TenantId];
        string? resourceTenantId = resourceTenant as string;
        if (string.IsNullOrWhiteSpace(resourceTenantId))
        {
            return Task.CompletedTask;
        }

        if (string.Equals(userTenantId, resourceTenantId, StringComparison.Ordinal))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
