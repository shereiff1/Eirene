using Hangfire.Dashboard;
using Microsoft.AspNetCore.Authentication;
using System.Threading.Tasks;

namespace Eirene.API.Filters;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        
        // Explicitly authenticate against the HangfireCookie scheme
        // We use GetAwaiter().GetResult() because Authorize is synchronous
        var authResult = httpContext.AuthenticateAsync("HangfireCookie").GetAwaiter().GetResult();
        
        if (authResult.Succeeded && authResult.Principal != null)
        {
            httpContext.User = authResult.Principal;
        }

        var isAuthenticated = httpContext.User?.Identity?.IsAuthenticated ?? false;
        var isAdmin = httpContext.User?.IsInRole("Admin") ?? false;

        return isAuthenticated && isAdmin;
    }
}
