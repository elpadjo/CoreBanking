using Hangfire;

namespace CoreBanking.API.Extensions;

public static class HangfireDashboardExtensions
{
    public static IApplicationBuilder UseHangfireDashboardWithAuth(this IApplicationBuilder app)
    {
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            DashboardTitle = "CoreBanking Job Dashboard",
            DisplayStorageConnectionString = false,
            Authorization = new[] { new HangfireAuthorizationFilter() },
            StatsPollingInterval = 5000, // 5 seconds
            AppPath = "/", // Back to site URL
            IgnoreAntiforgeryToken = true
        });

        return app;
    }
}