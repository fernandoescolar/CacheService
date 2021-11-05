using System.Text.Json;

namespace CacheService
{
    internal class DefaultCacheSerializer : ICacheSerializer
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new();

        public async Task<T?> DeserializeAsync<T>(byte[] bytes, CancellationToken cancellationToken)
        {
            if (bytes is null || bytes.Length == 0)
            {
                return default;
            }

            using var ms = new MemoryStream(bytes);
            var result = await JsonSerializer.DeserializeAsync<T>(ms, _jsonSerializerOptions, cancellationToken);
            return result;
        }

        public async Task<byte[]?> SerializeAsync<T>(T value, CancellationToken cancellationToken)
        {
            if (value is null)
            { 
                return null;
            }

            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, value, _jsonSerializerOptions, cancellationToken);
            return ms.ToArray();
        }
    }
}
