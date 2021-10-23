using Microsoft.Extensions.Caching.Distributed;

namespace CacheService.Background
{
    internal class DistributedJob<T> : Job<T>
    {
        private readonly IDistributedCache _distributeCache;

        public DistributedJob(IDistributedCache distributeCache, JobParameters<T> parameters)
            : base(parameters)
        {
            _distributeCache = distributeCache;
        }

        protected override async Task OnExecuteAsync(CancellationToken cancellationToken)
        {
            var value = await ValueGetter(cancellationToken);
            await _distributeCache.SetAsync(Key, value, Options);
        }
    }
}
