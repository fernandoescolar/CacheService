namespace CacheService
{
    public interface ICacheSerializer
    {
        Task<T?> DeserializeAsync<T>(byte[] bytes, CancellationToken cancellationToken);

        Task<byte[]> SerializeAsync<T>(T value, CancellationToken cancellationToken);
    }
}
