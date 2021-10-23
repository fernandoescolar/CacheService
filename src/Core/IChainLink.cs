namespace CacheService.Core
{
    public interface IChainLink
    {
        IChainLink? Next { get; set; }

        ValueTask<T?> HandleAsync<T>(ChainContext<T> context) where T: class;
    }
}