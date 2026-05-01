using Hangfire.Dashboard;

namespace Api.BackgroundJobs;

public sealed class OwnerDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        HttpContext httpContext = context.GetHttpContext();
        return httpContext.User.Identity?.IsAuthenticated is true
            && httpContext.User.IsInRole("Owner");
    }
}
