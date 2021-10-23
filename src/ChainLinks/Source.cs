using CacheService.Core;

namespace CacheService.Background
{
    internal class Source : ChainLink
    {
        protected override ValueTask<T?> OnGetAsync<T>(ChainContext<T> context) where T : class
            => context.ValueGetter(context.CancellationToken);
    }
}
