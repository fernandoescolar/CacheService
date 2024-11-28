namespace CacheService.Core;

internal sealed class FastJsonSerializer : ICacheSerializer
{
    public T? Deserialize<T>(byte[] bytes)
    {
        if (bytes.Length == 0)
        {
            return default;
        }

        var buffer = new ReadOnlySequence<byte>(bytes, 0, bytes.Length);
        var reader = new Utf8JsonReader(buffer);
        return JsonSerializer.Deserialize<T>(ref reader);
    }

    public void Serialize<T>(T value, IBufferWriter<byte> target)
    {
        if (value is null)
        {
            return;
        }

        using var writer = new Utf8JsonWriter(target);
        JsonSerializer.Serialize(writer, value);
    }
}
