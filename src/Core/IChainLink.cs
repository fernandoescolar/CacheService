namespace CacheService.Core;

/// <summary>
/// ChainLink inside the Chain Of Responsibility pattern.
/// </summary>
public interface IChainLink
{
    /// <summary>
    /// Order of the ChainLink inside the chain.
    /// </summary>
    ushort Order { get; }

    /// <summary>
    /// Next ChainLink in the chain.
    /// </summary>
    IChainLink? Next { get; set; }

    /// <summary>
    /// Executes the ChainLink.
    /// </summary>
    /// <typeparam name="T">The type os the value to process in the chain.</typeparam>
    /// <param name="context">The execution context.</param>
    /// <returns>The value get in the process.</returns>
    ValueTask<T?> HandleAsync<T>(ChainContext<T> context) where T : class;
}
