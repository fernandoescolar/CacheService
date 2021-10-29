using CacheService.Extensions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace CacheService.Background
{
    internal class MemoryJob<T> : Job<T>
    {
        private readonly MemoryCacheFactory _factory;

        public MemoryJob(MemoryCacheFactory factory, JobParameters<T> parameters) 
            : base(parameters)
        {
            _factory = factory;
        }

        protected override async Task OnExecuteAsync(CancellationToken cancellationToken)
        {
            var memoryCache = _factory();
            var value = await ValueGetter(cancellationToken);
            memoryCache.Set(Key, value, Options);
        }
    }
}
