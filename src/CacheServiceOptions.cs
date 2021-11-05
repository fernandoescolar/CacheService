namespace CacheService
{
    public class CacheServiceOptions 
    {
        public CacheOptions Distributed { get; set; } = new();

        public CacheOptions Memory { get; set; } = new();
    }
}
