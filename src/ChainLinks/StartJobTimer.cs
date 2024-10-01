namespace CacheService.ChainLinks;

internal class StartJobTimer : IChainLink
{

#pragma warning disable IDE0060 // Remove unused parameter
    // this is a dummy chain link used to start the job timer
    public StartJobTimer(JobTimer timer)
#pragma warning restore IDE0060 // Remove unused parameter

    {
    }

    public ushort Order => 0;

    public IChainLink? Next { get; set; }

    public ValueTask<T?> HandleAsync<T>(ChainContext<T> context) where T : class
    {
        if (Next is not null)
            return Next.HandleAsync(context);

        return ValueTask.FromResult<T?>(default);
    }
}
