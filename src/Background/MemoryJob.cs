namespace CacheService.Background;

internal class MemoryJob<T> : Job<T>
{
    private readonly MemoryCacheFactory _factory;
    private readonly ILogger<MemoryJob<T>> _logger;

    public MemoryJob(MemoryCacheFactory factory, JobParameters<T> parameters, ILogger<MemoryJob<T>> logger)
        : base(parameters)
    {
        _factory = factory;
        _logger = logger;
    }

    protected override async Task OnExecuteAsync(CancellationToken cancellationToken)
    {
        var memoryCache = _factory();
        if (memoryCache is null)
        {
            _logger.ExpectedIMemoryCacheButGotNull();
            return;
        }

        var value = await ValueGetter(cancellationToken);
        memoryCache.Set(Key, value, Options);
    }
}
