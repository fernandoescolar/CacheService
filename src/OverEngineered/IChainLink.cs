namespace CacheService.OverEngineered
{
    public interface IChainLink
    {
        IChainLink? Previous { get; set; }

        IChainLink? Next { get; set; }

        Task<T?> HandleAsync<T>(ChainContext<T> context) where T: class;

        Task CallbackAsync<T>(ChainContext<T> context);
    }
}