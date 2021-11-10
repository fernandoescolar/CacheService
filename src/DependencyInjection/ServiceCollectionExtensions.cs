namespace Microsoft.Extensions.DependencyInjection;

using Microsoft.Extensions.DependencyInjection.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the <see cref="ICacheService" />.
    /// </summary>
    /// <param name="services">The current <see cref="IServiceCollection" />.</param>
    /// <param name="configure">Configuration method.</param>
    /// <returns>The current <see cref="IServiceCollection" /></returns>
    public static IServiceCollection AddCacheService(this IServiceCollection services, Action<CacheServiceConfiguration>? configure = null)
    {
        var configuration = new CacheServiceConfiguration();
        configure?.Invoke(configuration);

        services.AddSingleton(configuration);
        services.TryAddSingleton<ICacheSerializer, DefaultCacheSerializer>();
        services.TryAddTransient<ICacheService, DefaultCacheService>();
        services.AddTransient<IChainLink, Source>();

        if (configuration.UseMemoryCache)
        {
            services.AddTransient<IChainLink, Memory>();
        }

        if (configuration.UseDistributedCache)
        {
            services.AddTransient<IChainLink, Distributed>();
        }

        if (configuration.BackgroundJobMode != BackgroundJobMode.None)
        {
            services.AddBackgroundJobDependencies();
        }

        if (configuration.BackgroundJobMode == BackgroundJobMode.HostedService)
        {
            services.AddBackgroundJobHostedService();
        }

        if (configuration.BackgroundJobMode == BackgroundJobMode.Timer)
        {
            services.AddBackgroundJobTimer();
        }

        return services;
    }

    private static IServiceCollection AddBackgroundJobDependencies(this IServiceCollection services)
    {
        services.AddTransient<IChainLink, AddOrUpdateJob>();

        services.TryAddSingleton<IJobManager, DefaultJobManager>();

        services.AddSingleton<MemoryCacheFactory>(s => () => s.GetRequiredService<IMemoryCache>());
        services.AddSingleton<DistributedCacheFactory>(s => () => s.GetRequiredService<IDistributedCache>());
        services.AddSingleton<CacheSerializerFactory>(s => () => s.GetRequiredService<ICacheSerializer>());

        return services;
    }

    private static IServiceCollection AddBackgroundJobHostedService(this IServiceCollection services)
        => services.AddHostedService<JobHostedService>();

    private static IServiceCollection AddBackgroundJobTimer(this IServiceCollection services)
    {
        services.AddSingleton<IChainLink, StartJobTimer>();
        services.AddSingleton<JobTimer>();
        return services;
    }
}
