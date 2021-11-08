using CacheService.Core;

namespace CacheService.ChainLinks
{
    internal class Source : ChainLink
    {
        public Source() : base(30)
        {
        }

        protected override ValueTask<T?> OnGetAsync<T>(ChainContext<T> context) where T : class
            => context.ValueGetter(context.CancellationToken);
    }
}
