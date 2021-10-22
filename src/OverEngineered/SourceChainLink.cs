namespace CacheService.OverEngineered
{
    public class SourceChainLink : ChainLink
    {
        protected override Task<T?> OnGetAsync<T>(ChainContext<T> context)  where T: class
            => context.ValueGetter(context.CancellationToken);

        protected override Task OnSetAsync<T>(ChainContext<T> context)
            => Task.CompletedTask;
    }
}
