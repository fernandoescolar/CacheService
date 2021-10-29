namespace CacheService.Core
{
    public interface IChainLink
    {
        ushort Order { get; }

        IChainLink? Next { get; set; }

        ValueTask<T?> HandleAsync<T>(ChainContext<T> context) where T: class;
    }
}