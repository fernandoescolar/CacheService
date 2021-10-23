using Microsoft.Extensions.Caching.Memory;
using CacheService.Extensions;
using Microsoft.Extensions.Caching.Distributed;

namespace CacheService.Background
{
    internal class MemoryJob<T> : Job<T>
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryJob(IMemoryCache memoryCache, JobParameters<T> parameters) 
            : base(parameters)
        {
            _memoryCache = memoryCache;
        }

        protected override async Task OnExecuteAsync(CancellationToken cancellationToken)
        {
            var value = await ValueGetter(cancellationToken);
            _memoryCache.Set(Key, value, Options);
        }
    }
}
