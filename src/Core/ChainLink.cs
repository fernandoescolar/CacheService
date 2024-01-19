namespace CacheService.Core;

internal abstract class ChainLink : IChainLink
{
    private readonly ushort _order;

    protected ChainLink(ushort order)
    {
        _order = order;
    }

    public IChainLink? Next { get; set; }

    public ushort Order => _order;

    public virtual async ValueTask<T?> HandleAsync<T>(ChainContext<T> context) where T : class
    {
        if (!context.Options.ForceRefresh)
        {
            context.Value = await OnGetAsync(context);
        }

        if (context.Value is null && Next is not null)
        {
            context.Value = await Next.HandleAsync(context);
            await OnSetAsync(context);
        }

        return context.Value;
    }

    protected virtual ValueTask<T?> OnGetAsync<T>(ChainContext<T> context) where T : class
        => ValueTask.FromResult<T?>(default);

    protected virtual Task OnSetAsync<T>(ChainContext<T> context)
        => Task.CompletedTask;
}
