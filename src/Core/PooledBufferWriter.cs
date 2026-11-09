namespace CacheService.Core;

internal sealed class PooledBufferWriter : IBufferWriter<byte>, IDisposable
{
    private byte[] _buffer;
    private int _position;

    public PooledBufferWriter()
    {
        _buffer = [];
    }

    public void Advance(int count)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        if (_position + count > _buffer.Length)
        {
            throw new InvalidOperationException("Cannot advance past the end of the buffer.");
        }

        _position += count;
    }

    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        Resize(sizeHint);
        return _buffer.AsMemory(_position);
    }

    public Span<byte> GetSpan(int sizeHint = 0)
    {
        Resize(sizeHint);
        return _buffer.AsSpan(_position);
    }

    public ReadOnlyMemory<byte> AsReadOnlyMemory()
    {
        return new ReadOnlyMemory<byte>(_buffer, 0, _position);
    }

    public byte[] ToArray()
    {
        if (_position == 0)
        {
            return [];
        }

         return _buffer.AsSpan(0, _position).ToArray();
    }

    public void Dispose()
    {
        ReleaseBytes();
    }

    private void ReleaseBytes()
    {
        if (_buffer.Length > 0)
        {
            ArrayPool<byte>.Shared.Return(_buffer);
        }
    }

    private void Resize(int sizeHint)
    {
        if (sizeHint < 1)
        {
            sizeHint = 1;
        }

        if (_buffer.Length - _position >= sizeHint)
        {
            // there is enough free space: renting (and copying to) a bigger buffer is not needed
            return;
        }

        var newSize = Math.Max(_buffer.Length * 2, sizeHint + _position);
        var newBuffer = ArrayPool<byte>.Shared.Rent(newSize);
        if (_position > 0)
        {
            _buffer.AsSpan(0, _position).CopyTo(newBuffer);
        }

        ReleaseBytes();
        _buffer = newBuffer;
    }
}
