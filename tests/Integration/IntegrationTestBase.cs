using CacheService.Configuration;
using CacheService.Tests.Doubles;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;

namespace CacheService.Tests.Integration
{
    public abstract class IntegrationTestBase : IDisposable
    {
        private bool disposed = false;

        protected IntegrationTestBase()
        {
            MemoryCache = new DummyMemoryCache();
            DistributedCache = new DummyDistributedCache();

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<IMemoryCache>(MemoryCache);
            services.AddSingleton<IDistributedCache>(DistributedCache);
            services.AddCacheService(OnConfigure);

            ServiceProvider = services.BuildServiceProvider();
            Target = ServiceProvider.GetRequiredService<ICacheService>();
            Serializer = ServiceProvider.GetRequiredService<ICacheSerializer>();
            JobHostedService = ServiceProvider.GetRequiredService<IHostedService>();
            TestScope = new CancellationTokenSource();
        }

        protected ICacheService Target { get; private set; }

        protected ServiceProvider ServiceProvider { get; private set; }

        protected ICacheSerializer Serializer { get; private set; }

        protected IHostedService JobHostedService { get; private set; }

        protected DummyMemoryCache MemoryCache { get; private set; }

        protected DummyDistributedCache DistributedCache { get; private set; }

        protected CancellationTokenSource TestScope { get; private set; }

        protected CancellationToken CancellationToken => TestScope.Token;

        public void Dispose()
            => Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;

            ServiceProvider.Dispose();
            TestScope?.Cancel();
            TestScope?.Dispose();
            disposed = true;
        }

        protected virtual void OnConfigure(CacheServiceConfiguration configuration)
        {
        }
    }
}
