namespace CacheService.Configuration
{
    public class CacheServiceConfiguration
    {
        public CacheServiceOptions DefaultOption { get; set; } = new CacheServiceOptions();

        public bool UseMemoryCache { get; set; } = true;

        public bool UseDistributedCache { get; set; } = true;

        public bool UseJobHostedService { get; set; } = true;
    }
}
