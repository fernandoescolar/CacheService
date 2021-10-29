using CacheService;
using CacheService.Background;
using CacheService.ChainLinks;
using CacheService.Core;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCacheService(this IServiceCollection services)
        {
            services.AddTransient<IChainLink, AddOrUpdateJob>();
            services.AddTransient<IChainLink, Memory>();
            services.AddTransient<IChainLink, Distributed>();
            services.AddTransient<IChainLink, Source>();

            services.TryAddSingleton<ICacheSerializer, DefaultCacheSerializer>();
            services.TryAddTransient<ICacheService, DefaultCacheService>();

            services.AddCacheServiceBackground();

            return services;
        }

        private static IServiceCollection AddCacheServiceBackground(this IServiceCollection services)
        {
            services.TryAddSingleton<IJobManager, DefaultJobManager>();

            services.AddSingleton<MemoryCacheFactory>(s => () => s.GetRequiredService<IMemoryCache>());
            services.AddSingleton<DistributedCacheFactory>(s => () => s.GetRequiredService<IDistributedCache>());
            services.AddSingleton<CacheSerializerFactory>(s => () => s.GetRequiredService<ICacheSerializer>());

            services.AddHostedService<JobHostedService>();

            return services;
        }
    }
}
