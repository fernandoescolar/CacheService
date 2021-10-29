namespace CacheService
{
    public class CacheServiceOptions 
    {
        public static readonly CacheServiceOptions Default = new();

        public CacheOptions Distributed { get; set; } = new();

        public CacheOptions Memory { get; set; } = new();
    }
}
