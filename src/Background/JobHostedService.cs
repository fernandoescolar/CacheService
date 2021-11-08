using CacheService.Configuration;
using Microsoft.Extensions.Hosting;

namespace CacheService.Background
{
    internal class JobHostedService : BackgroundService, IDisposable
    {
        private readonly IJobManager _bgManager;
        private readonly int _intervalInMs;

        public JobHostedService(IJobManager bgManager, CacheServiceConfiguration configuration)
        {
            _bgManager = bgManager;
            _intervalInMs = (int)configuration.BackgroundJobInterval.TotalMilliseconds;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(_intervalInMs, stoppingToken);
                await _bgManager.ExecuteJobsAsync(stoppingToken);
            }
        }
    }
}
