namespace CacheService.Tests.Doubles;

public sealed class DummyObject : IEquatable<DummyObject>, IEqualityComparer<DummyObject>
{
    public string Id { get; set; }

    public DummyObject() : this(Guid.NewGuid().ToString())
    {
    }

    public DummyObject(string id)
    {
        Id = id;
    }

    public override bool Equals(object? obj)
        => Equals(obj as DummyObject);

    public bool Equals(DummyObject? obj)
    {
        if (obj is null) return false;
        if (Id is null) return false;

        return Id.Equals(obj.Id);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public bool Equals(DummyObject? x, DummyObject? y)
    {
        if (x is null) return false;
        if (y is null) return false;

        return x.Id.Equals(y.Id);
    }

    public int GetHashCode([DisallowNull] DummyObject obj)
    {
        return obj.Id.GetHashCode();
    }

}