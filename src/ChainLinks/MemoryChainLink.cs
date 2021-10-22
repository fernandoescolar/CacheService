using CacheService.Core;
using Microsoft.Extensions.Caching.Memory;

namespace CacheService.ChainLinks
{
    internal class MemoryChainLink : ChainLink
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryChainLink(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        protected override Task<T?> OnGetAsync<T>(ChainContext<T> context)  where T: class
            => Task.FromResult(_memoryCache.TryGetValue(context.Key, out var value) ? (T?)value : default);

        protected override Task OnSetAsync<T>(ChainContext<T> context)
        {
            using var entry = _memoryCache.CreateEntry(context.Key);
            entry.Value = context.Value;
            entry.AbsoluteExpiration = context.Options.AbsoluteExpiration;
            return Task.CompletedTask;
        }
    }
}
