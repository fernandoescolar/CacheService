namespace CacheService.OverEngineered
{
    public class DefaultCacheService : ChainOfResponsibility, ICacheService
    {
        public DefaultCacheService(MemoryChainLink memoryChainLink, DistributedChainLink distributedChainLink, SourceChainLink sourceChainLink)
        {
            memoryChainLink = memoryChainLink ?? throw new System.ArgumentNullException(nameof(memoryChainLink));
            distributedChainLink = distributedChainLink ?? throw new System.ArgumentNullException(nameof(distributedChainLink));
            sourceChainLink = sourceChainLink ?? throw new System.ArgumentNullException(nameof(sourceChainLink));

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
