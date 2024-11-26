namespace CacheService.Tests.Doubles;

internal class DummyChainLink : ChainLink
{
    private readonly object? _value = default;

    public DummyChainLink(ushort order) : base(order)
    {
    }

    public DummyChainLink() : this(ushort.MaxValue)
    {
    }

    public DummyChainLink(object value) : this()
    {
        _value = value;
    }

    public DummyChainLink(object value, ushort order) : this(order)
    {
        _value = value;
    }

    protected override ValueTask<T?> OnGetAsync<T>(ChainContext<T> context) where T : class
        => ValueTask.FromResult(_value as T);
}