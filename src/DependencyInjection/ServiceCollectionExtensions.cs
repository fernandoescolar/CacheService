
namespace Microsoft.Extensions.DependencyInjection;

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
        services.AddCoreDependencies(configure);
        services.AddBackgroundDependencies();

        return services;
    }

    private static IServiceCollection AddCoreDependencies(this IServiceCollection services, Action<CacheServiceConfiguration>? configure = null)
    {
        services.Configure<CacheServiceConfiguration>(options => configure?.Invoke(options));
        services.AddSingleton<Source>();
        services.AddSingleton<Memory>();
        services.AddSingleton<Distributed>();
        services.TryAddSingleton<ICacheSerializer, EmptyCacheSerializer>();
        services.TryAddSingleton<ICacheService>(sp =>
        {
            var configuration = sp.GetRequiredService<IOptions<CacheServiceConfiguration>>();
            var chainLinks = new List<IChainLink>
            {
                sp.GetRequiredService<Source>()
            };

            if (configuration.Value.UseMemoryCache)
            {
                chainLinks.Add(sp.GetRequiredService<Memory>());
            }

            if (configuration.Value.UseDistributedCache)
            {
                chainLinks.Add(sp.GetRequiredService<Distributed>());
            }

            if (configuration.Value.BackgroundJobMode != BackgroundJobMode.None)
            {
                chainLinks.Add(sp.GetRequiredService<AddOrUpdateJob>());
            }

            if (configuration.Value.BackgroundJobMode == BackgroundJobMode.Timer)
            {
                chainLinks.Add(sp.GetRequiredService<StartJobTimer>());
            }

            return ActivatorUtilities.CreateInstance<DefaultCacheService>(sp, configuration, chainLinks);
        });

        return services;
    }

    private static IServiceCollection AddBackgroundDependencies(this IServiceCollection services)
    {
        services.AddSingleton<StartJobTimer>();
        services.AddSingleton<AddOrUpdateJob>();

        services.TryAddSingleton<IJobManager, DefaultJobManager>();

        services.AddSingleton<MemoryCacheFactory>(s => () => s.GetRequiredService<IMemoryCache>());
        services.AddSingleton<DistributedCacheFactory>(s => () => s.GetRequiredService<IDistributedCache>());
        services.AddSingleton<CacheSerializerFactory>(s => () => s.GetRequiredService<ICacheSerializer>());
        services.AddSingleton<JobTimer>();
        services.AddSingleton<JobHostedService>();
        services.AddBackgroundJobHostedService();
        return services;
    }

    private static IServiceCollection AddBackgroundJobHostedService(this IServiceCollection services)
        => services.AddSingleton<IHostedService>(sp =>
        {
            var configuration = sp.GetRequiredService<IOptions<CacheServiceConfiguration>>().Value;
            return configuration.BackgroundJobMode switch
            {
                BackgroundJobMode.HostedService => sp.GetRequiredService<JobHostedService>(),
                _ => new EmptyHostedService()
            };
        });

    private sealed class EmptyHostedService : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
}
