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
        var value = await ValueGetter(cancellationToken);

        try
        {
            var distributedCache = _factory();
            var serializer = _serializerFactory();
            byte[] bytes;
            if (serializer is EmptyCacheSerializer)
            {
                bytes = FastJsonSerializer.Serialize(value);
            }
            else
            {
                bytes = await _serializerFactory().SerializeAsync(value, cancellationToken)?? [];
            }

            await distributedCache.SetAsync(Key, bytes, Options, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.CannotSet(Key, ex.Message);
        }
    }
}

internal static partial class DistributedJobLoggerExtensions
{
    [LoggerMessage(10, LogLevel.Warning, "Cannot set to DistributedCache with key: {Key} -> {ex}")]
    public static partial void CannotSet(this ILogger<DistributedJob<object>> logger, string key, string ex);

}