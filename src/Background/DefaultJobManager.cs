namespace CacheService.Background;

internal class DefaultJobManager : IJobManager
{
    private readonly ConcurrentDictionary<string, IJob> _jobs = new();
    private readonly CacheServiceConfiguration _configuration;
    private readonly MemoryCacheFactory _memoryCacheFactory;
    private readonly DistributedCacheFactory _distributedCacheFactory;
    private readonly CacheSerializerFactory _serializerFactory;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<DefaultJobManager> _logger;

    public DefaultJobManager(IOptions<CacheServiceConfiguration> configuration, MemoryCacheFactory memoryCacheFactory, DistributedCacheFactory distributedCacheFactory, CacheSerializerFactory serializerFactory, ILoggerFactory loggerFactory)
    {
        _configuration = configuration.Value;
        _memoryCacheFactory = memoryCacheFactory;
        _distributedCacheFactory = distributedCacheFactory;
        _serializerFactory = serializerFactory;
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<DefaultJobManager>();
    }

    public void AddOrUpdateJob<T>(string key, Func<CancellationToken, ValueTask<T?>> valueGetter, CacheServiceOptions options) where T : class
    {
        if (_configuration.BackgroundJobMode == BackgroundJobMode.None)
        {
            _logger.BackgroundJobModeIsNone();
            return;
        }

        if (_configuration.UseMemoryCache)
        {
            var memoryJob = new MemoryJob<T>(_memoryCacheFactory, new JobParameters<T>(key, options.Memory, valueGetter), _loggerFactory.CreateLogger<MemoryJob<T>>());
            _jobs.AddOrUpdate($"MemoryJob_{key}", memoryJob, (k, j) => j.UpdateJob(memoryJob));
        }

        if (_configuration.UseDistributedCache)
        {
            var distributedJob = new DistributedJob<T>(_distributedCacheFactory, _serializerFactory, new JobParameters<T>(key, options.Distributed, valueGetter), _loggerFactory.CreateLogger<DistributedJob<T>>());
            _jobs.AddOrUpdate($"DistributedJob_{key}", distributedJob, (k, j) => j.UpdateJob(distributedJob));
        }
    }

    public Task ExecuteJobsAsync(CancellationToken cancellationToken)
    {
        var tasks = _jobs.Where(x => x.Value.DueTime <= DateTime.UtcNow).ToList();
        return Parallel.ForEachAsync(tasks, ExecuteJobAsync);
    }

    private async ValueTask ExecuteJobAsync(KeyValuePair<string, IJob> job, CancellationToken cancellationToken)
    {
        if (_configuration.BackgroundJobIgnoreExceptions)
        {
            await SafeExecuteAsync(job.Value, cancellationToken);
        }
        else
        {
            await UnsafeExecuteAsync(job.Value, cancellationToken);
        }
    }

    private async ValueTask SafeExecuteAsync(IJob job, CancellationToken cancellationToken)
    {
        try
        {
            await job.ExecuteAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.UnexpectedBackgroundJobException(ex.ToString());
        }
    }

    private static ValueTask UnsafeExecuteAsync(IJob job, CancellationToken cancellationToken)
        =>new(job.ExecuteAsync(cancellationToken));
}
