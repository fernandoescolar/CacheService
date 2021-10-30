using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace CacheService.Background
{
    internal class DefaultJobManager : IJobManager
    {
        private readonly ConcurrentDictionary<string, IJob> _jobs = new();
        private readonly MemoryCacheFactory _memoryCacheFactory;
        private readonly DistributedCacheFactory _distributedCacheFactory;
        private readonly CacheSerializerFactory _serializerFactory;
        private readonly ILoggerFactory _loggerFactory;

        public DefaultJobManager(MemoryCacheFactory memoryCacheFactory, DistributedCacheFactory distributedCacheFactory, CacheSerializerFactory serializerFactory, ILoggerFactory loggerFactory)
        {
            _memoryCacheFactory = memoryCacheFactory;
            _distributedCacheFactory = distributedCacheFactory;
            _serializerFactory = serializerFactory;
            _loggerFactory = loggerFactory;
        }

        public void AddOrUpdateJob<T>(string key, Func<CancellationToken, ValueTask<T?>> valueGetter, CacheServiceOptions options) where T : class
        {
            var memoryJob = new MemoryJob<T>(_memoryCacheFactory, new JobParameters<T>(key, options.Memory, valueGetter));
            _jobs.AddOrUpdate($"MemoryJob_{key}", memoryJob, (k, j) => j.UpdateJob(memoryJob));

            var distributedJob = new DistributedJob<T>(_distributedCacheFactory, _serializerFactory, new JobParameters<T>(key, options.Distributed, valueGetter), _loggerFactory.CreateLogger<DistributedJob<T>>());
            _jobs.AddOrUpdate($"DistributedJob_{key}", distributedJob, (k, j) => j.UpdateJob(distributedJob));
        }

        public Task ExecuteJobsAsync(CancellationToken cancellationToken)
        {
            var tasks = _jobs.Where(x => x.Value.DueTime <= DateTime.UtcNow).ToList();
            return Parallel.ForEachAsync(tasks, ExecuteJobAsync);
        }

        private ValueTask ExecuteJobAsync(KeyValuePair<string, IJob> job, CancellationToken cancellationToken)
            => new(job.Value.ExecuteAsync(cancellationToken));
    }
}
