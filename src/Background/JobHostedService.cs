using Microsoft.Extensions.Hosting;

namespace CacheService.Background
{
    internal class JobHostedService : BackgroundService, IDisposable
    {
        private readonly IJobManager _bgManager;

        public JobHostedService(IJobManager bgManager)
        {
            _bgManager = bgManager;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
                await _bgManager.ExecuteJobsAsync(stoppingToken); 
            }
        }
    }
}
