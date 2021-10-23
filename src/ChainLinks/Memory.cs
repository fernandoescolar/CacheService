using CacheService.Core;
using Microsoft.Extensions.Caching.Memory;

namespace CacheService.ChainLinks
{
    internal class Memory : ChainLink
    {
        private readonly IMemoryCache _memoryCache;

        public Memory(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        protected override ValueTask<T?> OnGetAsync<T>(ChainContext<T> context)  where T: class
            => ValueTask.FromResult(_memoryCache.TryGetValue(context.Key, out var value) ? (T?)value : default);

        protected override Task OnSetAsync<T>(ChainContext<T> context)
        {
            using var entry = _memoryCache.CreateEntry(context.Key);
            entry.Value = context.Value;
            entry.AbsoluteExpiration = context.Options.Memory.AbsoluteExpiration;
            entry.SlidingExpiration = context.Options.Memory.SlidingExpiration;
            return Task.CompletedTask;
        }
    }
}
