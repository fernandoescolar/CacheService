using CacheService.Background;
using CacheService.ChainLinks;
using CacheService.Core;

namespace CacheService
{
    internal sealed class DefaultCacheService : ChainOfResponsibility, ICacheService
    {
        public DefaultCacheService(AddOrUpdateJob bgChainLink, Memory memoryChainLink, Distributed distributedChainLink, Source sourceChainLink)
        {
            bgChainLink = bgChainLink ?? throw new ArgumentNullException(nameof(bgChainLink));
            memoryChainLink = memoryChainLink ?? throw new ArgumentNullException(nameof(memoryChainLink));
            distributedChainLink = distributedChainLink ?? throw new ArgumentNullException(nameof(distributedChainLink));
            sourceChainLink = sourceChainLink ?? throw new ArgumentNullException(nameof(sourceChainLink));

            AddLink(bgChainLink);
            AddLink(memoryChainLink);
            AddLink(distributedChainLink);
            AddLink(sourceChainLink);
        }

        public ValueTask<T?> GetOrSetAsync<T>(string key, CacheServiceOptions options, Func<CancellationToken, ValueTask<T?>> getter, CancellationToken cancellationToken = default) where T: class
        {
            cancellationToken.ThrowIfCancellationRequested();

            var context = new ChainContext<T>(key, options, getter, cancellationToken);
            return HandleAsync(context);
        }
    }
}
