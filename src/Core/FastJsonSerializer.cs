namespace CacheService.Core;

internal sealed class FastJsonSerializer : ICacheSerializer
{
    public T? Deserialize<T>(byte[] bytes)
    {
        if (bytes.Length == 0)
        {
            return default;
        }

        // the span overload parses the payload once: the Utf8JsonReader one needs
        // a first pass to find the end of the value before deserializing it
        return JsonSerializer.Deserialize<T>(new ReadOnlySpan<byte>(bytes));
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
