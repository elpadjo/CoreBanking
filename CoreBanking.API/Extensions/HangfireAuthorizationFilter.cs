using Hangfire.Dashboard;

namespace CoreBanking.API.Extensions
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            // Only allow authenticated users with admin role
            return httpContext.User.Identity.IsAuthenticated &&
                   httpContext.User.IsInRole("Admin");
        }
    }
}
