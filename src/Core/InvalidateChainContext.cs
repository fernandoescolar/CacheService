namespace CacheService.Core;

internal class InvalidateChainContext : ChainContext<object>
{
    internal InvalidateChainContext(string key, CancellationToken cancellationToken = default)
        : base(key, options, getter, cancellationToken)
    {
    }

    private static readonly Func<CancellationToken, ValueTask<object?>> getter = ct => ValueTask.FromResult<object?>(default);

    private static readonly CacheServiceOptions options = new()
    {
        Memory = new()
        {
            AbsoluteExpiration = DateTimeOffset.MinValue,
            RefreshInterval = null
        },
        Distributed = new()
        {
            AbsoluteExpiration = DateTimeOffset.MinValue,
            RefreshInterval = null
        },
        ForceRefresh = true
    };
}
