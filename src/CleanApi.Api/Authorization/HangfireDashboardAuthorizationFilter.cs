using CleanApi.Domain.Authorization;
using Hangfire.Dashboard;

namespace CleanApi.Api.Authorization;

/// <summary>
/// Guards the Hangfire dashboard. Open in Development for convenience; in other environments it
/// requires an authenticated Administrator.
/// </summary>
public sealed class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        var environment = httpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();

        if (environment.IsDevelopment())
        {
            return true;
        }

        return httpContext.User.Identity?.IsAuthenticated == true
            && httpContext.User.IsInRole(Roles.Administrator);
    }
}
