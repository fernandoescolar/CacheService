namespace CacheService;

internal class EmptyCacheSerializer : ICacheSerializer
{
    public Task<T?> DeserializeAsync<T>(byte[] bytes, CancellationToken cancellationToken)
        => Task.FromResult<T?>(default);

    public Task<byte[]?> SerializeAsync<T>(T value, CancellationToken cancellationToken)
        => Task.FromResult<byte[]?>(default);
}