using CacheService.Core;

namespace CacheService
{
    internal sealed class DefaultCacheService : ChainOfResponsibility, ICacheService
    {
        public DefaultCacheService(IEnumerable<IChainLink> chainLinks)
        {
            chainLinks = chainLinks ?? throw new ArgumentNullException(nameof(chainLinks));

            foreach (var chainLink in chainLinks.OrderBy(x => x.Order))
            {
                AddLink(chainLink);
            }
        }

        public ValueTask<T?> GetOrSetAsync<T>(string key, CacheServiceOptions options, Func<CancellationToken, ValueTask<T?>> getter, CancellationToken cancellationToken = default) where T: class
        {
            cancellationToken.ThrowIfCancellationRequested();

            var context = new ChainContext<T>(key, options, getter, cancellationToken);
            return HandleAsync(context);
        }

        public Task InvalidateAsync(string key, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var options = new CacheServiceOptions();
            options.Memory.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(0);
            options.Distributed.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(0);

            var context = new ChainContext<object>(key, options, ct => ValueTask.FromResult<object?>(default), cancellationToken);
            return HandleAsync(context).AsTask();
        }
    }
}
