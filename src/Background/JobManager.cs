using System.Collections.Concurrent;

namespace CacheService.Background
{
    public class JobManager : IJobManager
    {
        private readonly ConcurrentDictionary<string, IJob> _jobs = new();

        public void AddOrUpdateJob<T>(string key, Func<CancellationToken, ValueTask<T?>> valueGetter, CacheServiceOptions options) where T : class
        {
            var jobDetail = new Job<T>();
            _jobs.AddOrUpdate(key, jobDetail, (k, j) => j.UpdateJob(jobDetail));
        }

        public Task ExecuteJobsAsync(CancellationToken cancellationToken)
        {
            var tasks = _jobs.Where(x => x.Value.DueTime <= DateTime.UtcNow);
            return Parallel.ForEachAsync(tasks, ExecuteJobAsync);
        }

        private ValueTask ExecuteJobAsync(KeyValuePair<string, IJob> job, CancellationToken cancellationToken)
            => new (job.Value.ExecuteAsync(cancellationToken));
    }
}
