namespace CacheService.Background;

internal class DefaultJobManager : IJobManager
{
    private readonly ConcurrentDictionary<string, IJob> _memoryJobs = new();
    private readonly ConcurrentDictionary<string, IJob> _distributedJobs = new();
    private readonly CacheServiceConfiguration _configuration;
    private readonly MemoryCacheFactory _memoryCacheFactory;
    private readonly DistributedCacheFactory _distributedCacheFactory;
    private readonly CacheSerializerFactory _serializerFactory;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<DefaultJobManager> _logger;

    // These flags avoid to look for a job in every cache read when there are no registered jobs.
    private volatile bool _hasMemoryJobs;
    private volatile bool _hasDistributedJobs;

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

        if (options.UseMemoryCache)
        {
            var memoryOptions = options.Memory;
            if (_hasMemoryJobs && _memoryJobs.TryGetValue(key, out var job))
            {
                UpdateJob(job, memoryOptions, valueGetter);
            }
            else if (memoryOptions.RefreshInterval is not null)
            {
                AddMemoryJob(key, memoryOptions, valueGetter);
            }
        }

        if (options.UseDistributedCache)
        {
            var distributedOptions = options.Distributed;
            if (_hasDistributedJobs && _distributedJobs.TryGetValue(key, out var job))
            {
                UpdateJob(job, distributedOptions, valueGetter);
            }
            else if (distributedOptions.RefreshInterval is not null)
            {
                AddDistributedJob(key, distributedOptions, valueGetter);
            }
        }
    }

    public Task ExecuteJobsAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var jobs = _memoryJobs.Concat(_distributedJobs)
                              .Select(x => x.Value)
                              .Where(x => x.DueTime <= now)
                              .ToList();

        return jobs.Count == 0 ? Task.CompletedTask : Parallel.ForEachAsync(jobs, ExecuteJobAsync);
    }

    private static void UpdateJob<T>(IJob job, CacheOptions options, Func<CancellationToken, ValueTask<T?>> valueGetter)
    {
        if (job is not Job<T> typedJob)
        {
            throw new InvalidOperationException($"otherJob doesn't is a JobDetails<{typeof(T).Name}>");
        }

        typedJob.UpdateJob(options, valueGetter);
    }

    // A job without RefreshInterval is never executed, so it is created only when it is needed:
    // these methods are out of the hot path.
    private void AddMemoryJob<T>(string key, CacheOptions options, Func<CancellationToken, ValueTask<T?>> valueGetter)
    {
        var memoryJob = new MemoryJob<T>(_memoryCacheFactory, new JobParameters<T>(key, options, valueGetter), _loggerFactory.CreateLogger<MemoryJob<T>>());
        _hasMemoryJobs = true;
        _memoryJobs.AddOrUpdate(key, memoryJob, (_, j) => j.UpdateJob(memoryJob));
    }

    private void AddDistributedJob<T>(string key, CacheOptions options, Func<CancellationToken, ValueTask<T?>> valueGetter)
    {
        var distributedJob = new DistributedJob<T>(_distributedCacheFactory, _serializerFactory, new JobParameters<T>(key, options, valueGetter), _loggerFactory.CreateLogger<DistributedJob<T>>());
        _hasDistributedJobs = true;
        _distributedJobs.AddOrUpdate(key, distributedJob, (_, j) => j.UpdateJob(distributedJob));
    }

    private ValueTask ExecuteJobAsync(IJob job, CancellationToken cancellationToken)
    {
        if (_configuration.BackgroundJobIgnoreExceptions)
        {
            return SafeExecuteAsync(job, cancellationToken);
        }
        else
        {
            return UnsafeExecuteAsync(job, cancellationToken);
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
