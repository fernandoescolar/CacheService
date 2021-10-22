using CacheService.ChainLinks;
using CacheService.Core;

namespace CacheService
{
    internal sealed class DefaultCacheService : ChainOfResponsibility, ICacheService
    {
        public DefaultCacheService(MemoryChainLink memoryChainLink, DistributedChainLink distributedChainLink, SourceChainLink sourceChainLink)
        {
            memoryChainLink = memoryChainLink ?? throw new ArgumentNullException(nameof(memoryChainLink));
            distributedChainLink = distributedChainLink ?? throw new ArgumentNullException(nameof(distributedChainLink));
            sourceChainLink = sourceChainLink ?? throw new ArgumentNullException(nameof(sourceChainLink));

            AddLink(memoryChainLink);
            AddLink(distributedChainLink);
            AddLink(sourceChainLink);
        }

        public Task<T?> GetOrSetAsync<T>(string key, CacheServiceOptions options, Func<CancellationToken, Task<T?>> getter, CancellationToken cancellationToken = default) where T: class
        {
            cancellationToken.ThrowIfCancellationRequested();

            var context = new ChainContext<T>(key, options, getter, cancellationToken);
            return HandleAsync(context);
        }
    }
}
