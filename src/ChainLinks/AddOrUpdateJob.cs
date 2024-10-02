namespace CacheService.ChainLinks;

internal class AddOrUpdateJob : IChainLink
{
    private readonly IJobManager _bgManager;

    public IChainLink? Next { get; set; }

    public ushort Order => 1;

    public AddOrUpdateJob(IJobManager bgManager)
    {
        _bgManager = bgManager;
    }

    public ValueTask<T?> HandleAsync<T>(ChainContext<T> context) where T : class
    {
        if (context is not InvalidateChainContext) // do not update or add job if it is an invalidation request
            _bgManager.AddOrUpdateJob(context.Key, context.ValueGetter, context.Options);

        if (Next is not null)
            return Next.HandleAsync(context);

        return ValueTask.FromResult<T?>(default);
    }
}
