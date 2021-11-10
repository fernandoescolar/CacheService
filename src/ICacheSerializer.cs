namespace CacheService;

/// <summary>
/// Cache Serializer use to serialize/deserialize values in <see cref="Microsoft.Extensions.Caching.Distributed.IDistributedCache"/>.
/// </summary>
public interface ICacheSerializer
{
    /// <summary>
    /// Deserializes a value from a byte array.
    /// </summary>
    /// <typeparam name="T">Type of the value to deserialize.</typeparam>
    /// <param name="bytes">The serialized data.</param>
    /// <param name="cancellationToken">Optional. The System.Threading.CancellationToken used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The deserialized value.</returns>
    Task<T?> DeserializeAsync<T>(byte[] bytes, CancellationToken cancellationToken);

    /// <summary>
    /// Serialize a value into a byte array.
    /// </summary>
    /// <typeparam name="T">The type of the value to serialize.</typeparam>
    /// <param name="value">The value to serialize.</param>
    /// <param name="cancellationToken">Optional. The System.Threading.CancellationToken used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The serailized value as a byte array.</returns>
    Task<byte[]?> SerializeAsync<T>(T value, CancellationToken cancellationToken);
}
