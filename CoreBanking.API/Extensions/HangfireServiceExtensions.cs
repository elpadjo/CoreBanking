using CoreBanking.API.Services;
using CoreBanking.Core.Interfaces;
using CoreBanking.Infrastructure.BackgroundJobs;
using Hangfire;
using Hangfire.SqlServer;

namespace CoreBanking.API.Extensions;

public static class HangfireServiceExtensions
{
    public static IServiceCollection AddHangfireServices(this IServiceCollection services, IConfiguration configuration)
    {
        var hangfireConfig = configuration.GetSection("Hangfire").Get<HangfireConfiguration>();

        // Register your filter first
        services.AddSingleton<LogJobFilter>();

        // Use the overload that provides IServiceProvider
        services.AddHangfire((provider, config) => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(hangfireConfig.ConnectionString, new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true,
                PrepareSchemaIfNecessary = true
            })
            .UseFilter(new AutomaticRetryAttribute { Attempts = hangfireConfig.RetryAttempts })
            .UseFilter(provider.GetRequiredService<LogJobFilter>())); // Resolve from DI

        // Add Hangfire background processing
        services.AddHangfireServer(options =>
        {
            options.WorkerCount = hangfireConfig.WorkerCount;
            options.Queues = new[] { "default", "critical", "low" };
            options.ServerName = $"CoreBanking-{Environment.MachineName}";
        });

        services.AddSingleton(provider => JobStorage.Current.GetMonitoringApi());
        services.AddSingleton<IHangfireService, HangfireService>();

        return services;
    }
}