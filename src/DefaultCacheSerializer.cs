using System.Text.Json;

namespace CacheService
{
    internal class DefaultCacheSerializer : ICacheSerializer
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions();

        public async Task<T?> DeserializeAsync<T>(byte[] bytes, CancellationToken cancellationToken)
        {
            using var ms = new MemoryStream(bytes);
            var result = await JsonSerializer.DeserializeAsync<T>(ms, _jsonSerializerOptions, cancellationToken);
            return result;
        }

        public async Task<byte[]> SerializeAsync<T>(T value, CancellationToken cancellationToken)
        {
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, value, _jsonSerializerOptions, cancellationToken);
            return ms.ToArray();
        }
    }
}
