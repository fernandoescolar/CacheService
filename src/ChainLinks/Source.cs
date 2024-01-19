namespace CacheService.ChainLinks;

internal class Source : ChainLink
{
    public Source() : base(30)
    {
    }

    public override async ValueTask<T?> HandleAsync<T>(ChainContext<T> context) where T : class
    {
        context.Value = await OnGetAsync(context);
        return context.Value;
    }

    protected override ValueTask<T?> OnGetAsync<T>(ChainContext<T> context) where T : class
        => context.ValueGetter(context.CancellationToken);
}
