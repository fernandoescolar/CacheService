namespace CacheService.ChainLinks;

internal class Memory : ChainLink
{
    private readonly IMemoryCache _memoryCache;

    public Memory(IMemoryCache memoryCache)
        : base(10)
    {
        _memoryCache = memoryCache;
    }

    protected override ValueTask<T?> OnGetAsync<T>(ChainContext<T> context) where T : class
        => ValueTask.FromResult(_memoryCache.TryGetValue(context.Key, out var value) ? (T?)value : default);

    protected override Task OnSetAsync<T>(ChainContext<T> context)
    {
        _memoryCache.Set(context.Key, context.Value, context.Options.Memory);
        return Task.CompletedTask;
    }
}
