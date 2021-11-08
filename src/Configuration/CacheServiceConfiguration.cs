namespace CacheService.Configuration
{
    public class CacheServiceConfiguration
    {
        public CacheServiceOptions DefaultOption { get; set; } = new CacheServiceOptions();

        public bool UseMemoryCache { get; set; } = true;

        public bool UseDistributedCache { get; set; } = true;

        public BackgroundJobMode BackgroundJobMode { get; set; } = BackgroundJobMode.HostedService;

        public TimeSpan BackgroundJobInterval { get; set; } = TimeSpan.FromMinutes(1);
    }
}
