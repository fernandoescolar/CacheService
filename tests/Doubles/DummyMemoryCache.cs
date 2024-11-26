namespace CacheService.Tests.Doubles;

public sealed class DummyMemoryCache : Dictionary<string, DummyCacheEntry>, IMemoryCache
{
    public ICacheEntry CreateEntry(object key)
    {
        var skey = key?.ToString() ?? string.Empty;
        var entry = new DummyCacheEntry(skey);
        if (!ContainsKey(skey))
        {
            Add(entry.KeyString, entry);
        }
        else
        {
            this[skey] = entry;
        }

        return entry;
    }

    public void Dispose()
    {
    }

    void IMemoryCache.Remove(object key)
    {
        base.Remove(key?.ToString() ?? string.Empty);
    }

    bool IMemoryCache.TryGetValue(object key, out object? value)
    {
        var k = key?.ToString() ?? string.Empty;
        if (base.ContainsKey(k))
        {
            value = base[k].Value;
            return true;
        }

        value = default;
        return false;
    }
}
