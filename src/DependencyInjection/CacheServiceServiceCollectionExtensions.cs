
namespace Microsoft.Extensions.DependencyInjection;

public static class CacheServiceServiceCollectionExtensions
{
    /// <summary>
    /// Adds the <see cref="ICacheService" />.
    /// </summary>
    /// <param name="services">The current <see cref="IServiceCollection" />.</param>
    /// <param name="configure">Configuration method.</param>
    /// <returns>The current <see cref="IServiceCollection" /></returns>
    public static IServiceCollection AddCacheService(this IServiceCollection services, Action<CacheServiceConfiguration>? configure = null)
    {
        services.Configure<CacheServiceConfiguration>(options => configure?.Invoke(options));
        services.AddBackgroundDependencies();
        services.AddDelegatedFactories();
        services.AddCacheServiceFactory();

        return services;
    }

    private static IServiceCollection AddCacheServiceFactory(this IServiceCollection services)
    {
        services.AddSingleton<UglyCacheService>();
        services.TryAddSingleton<ICacheService>(sp =>
        {
            var configuration = sp.GetRequiredService<IOptions<CacheServiceConfiguration>>();
            if (configuration.Value.BackgroundJobMode == BackgroundJobMode.Timer)
            {
                // initialize timer
                _ = sp.GetRequiredService<JobTimer>();
            }

            return sp.GetRequiredService<UglyCacheService>();
        });

        return services;
    }

    private static IServiceCollection AddDelegatedFactories(this IServiceCollection services)
    {
        services.AddSingleton<MemoryCacheFactory>(sp => () =>
        {
             var configuration = sp.GetRequiredService<IOptions<CacheServiceConfiguration>>();
             return configuration.Value.UseMemoryCache ? sp.GetRequiredService<IMemoryCache>() : default;
        });
        services.AddSingleton<DistributedCacheFactory>(sp => () =>
        {
             var configuration = sp.GetRequiredService<IOptions<CacheServiceConfiguration>>();
             return configuration.Value.UseDistributedCache ? sp.GetRequiredService<IDistributedCache>() : default;
        });
        services.AddSingleton<JobManagerFactory>(sp => () =>
        {
             var configuration = sp.GetRequiredService<IOptions<CacheServiceConfiguration>>();
             return configuration.Value.BackgroundJobMode != BackgroundJobMode.None ? sp.GetRequiredService<IJobManager>() : default;
        });
        services.AddSingleton<CacheSerializerFactory>(sp => () => sp.GetService<ICacheSerializer>());
        services.AddSingleton<DelegatedFactories>();

        return services;
    }

    private static IServiceCollection AddBackgroundDependencies(this IServiceCollection services)
    {
        services.TryAddSingleton<IJobManager, DefaultJobManager>();
        services.AddSingleton<JobTimer>();
        services.AddSingleton<JobHostedService>();
        services.AddSingleton<IHostedService>(sp =>
        {
            var configuration = sp.GetRequiredService<IOptions<CacheServiceConfiguration>>().Value;
            return configuration.BackgroundJobMode switch
            {
                BackgroundJobMode.HostedService => sp.GetRequiredService<JobHostedService>(),
                _ => new EmptyHostedService()
            };
        });

        return services;
    }

    private sealed class EmptyHostedService : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
}
