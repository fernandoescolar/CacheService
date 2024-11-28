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
    /// <returns>The deserialized value.</returns>
    T? Deserialize<T>(byte[] bytes);


    /// <summary>
    /// Serialize a value into a byte array.
    /// </summary>
    /// <typeparam name="T">The type of the value to serialize.</typeparam>
    /// <param name="value">The value to serialize.</param>
    /// <param name="target">The target buffer to write the serialized data.</param>
    void Serialize<T>(T value, IBufferWriter<byte> target);
}
