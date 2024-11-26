namespace CacheService;

internal static class FastJsonSerializer
{
    private static readonly ArrayPool<byte> ReaderPool = ArrayPool<byte>.Shared;
    private static readonly ArrayBufferWriterPool WriterPool = new (100);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? Deserialize<T>(byte[] bytes)
    {
        if (bytes is null || bytes.Length == 0)
        {
            return default;
        }

        var buffer = ReaderPool.Rent(bytes.Length);
        try
        {
            new ReadOnlySequence<byte>(bytes).CopyTo(buffer);
            var sequence = new ReadOnlySequence<byte>(buffer, 0, buffer.Length);
            var reader = new Utf8JsonReader(sequence);
            var result = JsonSerializer.Deserialize<T>(ref reader);
            return result;
        }
        finally
        {
            ReaderPool.Return(buffer);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] Serialize<T>(T value)
    {
        if (value is null)
        {
            return [];
        }

        var buffer = WriterPool.Rent();
        try
        {
            using var writer = new Utf8JsonWriter(buffer);
            JsonSerializer.Serialize(writer, value);
            return buffer.WrittenMemory.ToArray();
        }
        finally
        {
            WriterPool.Return(buffer);
        }
    }
}

internal class ArrayBufferWriterPool
{
    private readonly ConcurrentStack<ArrayBufferWriter<byte>> _stack = new();
    public ArrayBufferWriterPool(int capacity)
    {
        for (var i = 0; i < capacity; i++)
        {
            _stack.Push(new ArrayBufferWriter<byte>(2147483591));
        }
    }

    public ArrayBufferWriter<byte> Rent()
    {
        if (_stack.TryPop(out var buffer))
        {
            return buffer;
        }

        return new ArrayBufferWriter<byte>(2147483591);
    }

    public void Return(ArrayBufferWriter<byte> buffer)
    {
        buffer.Clear();
        _stack.Push(buffer);
    }
}