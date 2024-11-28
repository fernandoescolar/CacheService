namespace CacheService.Background;

internal class DistributedJob<T> : Job<T>
{
    private readonly DistributedCacheFactory _factory;
    private readonly CacheSerializerFactory _serializerFactory;
    private readonly ILogger<DistributedJob<T>> _logger;

    public DistributedJob(DistributedCacheFactory distributedFactory, CacheSerializerFactory serializerFactory, JobParameters<T> parameters, ILogger<DistributedJob<T>> logger)
        : base(parameters)
    {
        _factory = distributedFactory;
        _serializerFactory = serializerFactory;
        _logger = logger;
    }

    protected override async Task OnExecuteAsync(CancellationToken cancellationToken)
    {
        var distributedCache = _factory();
        if (distributedCache is null)
        {
            _logger.ExpectedIDistributedCacheButGotNull();
            return;
        }

        var value = await ValueGetter(cancellationToken);
        try
        {
            var serializer = _serializerFactory();
            using var buffer = new PooledBufferWriter();
            serializer.Serialize(value, buffer);
            await distributedCache.SetAsync(Key, buffer.ToArray(), Options, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.CannotSetDistributedCache(Key, ex.Message);
        }
    }
}
