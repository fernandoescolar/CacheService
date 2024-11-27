namespace CacheService.Core;

internal static class FastJsonSerializer
{
    private const int MaxBufferSize = 2147483591;
    private static ConcurrentStack<ArrayBufferWriter<byte>> _bufferPool = new();

    static FastJsonSerializer()
    {
        for (var i = 0; i < 256; i++)
        {
            _bufferPool.Push(new ArrayBufferWriter<byte>(MaxBufferSize));
        }
    }

    public static T? Deserialize<T>(byte[] bytes)
    {
        if (bytes.Length == 0)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(bytes);
    }

    public static byte[] Serialize<T>(T value)
    {
        if (value is null)
        {
            return [];
        }

        ArrayBufferWriter<byte> buffer = _bufferPool.TryPop(out var b) ? b : new ArrayBufferWriter<byte>(MaxBufferSize);
        try
        {
            using var writer = new Utf8JsonWriter(buffer);
            JsonSerializer.Serialize(writer, value);
            return buffer.WrittenMemory.ToArray();
        }
        finally
        {
            if (_bufferPool.Count < 256)
            {
                buffer.Clear();
                _bufferPool.Push(buffer);
            }
        }
    }
}
