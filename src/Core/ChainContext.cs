namespace CacheService.Core
{
    public class ChainContext<T>
    {
        public string Key { get; }

        public CacheServiceOptions Options { get; }

        public Func<CancellationToken, ValueTask<T?>> ValueGetter { get; }

        public CancellationToken CancellationToken { get; }

        public T? Value { get; set; }

        public ChainContext(string key, CacheServiceOptions options, Func<CancellationToken, ValueTask<T?>> valueGetter, CancellationToken cancellationToken = default)
        {
            Key = key;
            Options = options;
            ValueGetter = valueGetter;
            CancellationToken = cancellationToken;
        }
    }
}