using CacheService.Core;

namespace CacheService.ChainLinks
{
    internal class SourceChainLink : ChainLink
    {
        protected override Task<T?> OnGetAsync<T>(ChainContext<T> context)  where T: class
            => context.ValueGetter(context.CancellationToken);
    }
}
