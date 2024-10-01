using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;

namespace CacheService.Tests.Doubles;

public sealed class DummyCacheEntry : ICacheEntry
{
    public DummyCacheEntry(string key)
    {
        KeyString = key;
    }

    public object Key => KeyString;

    public string KeyString { get; set; }

    public object? Value { get; set; }

    public DateTimeOffset? AbsoluteExpiration { get; set; }

    public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }

    public TimeSpan? SlidingExpiration { get; set; }

    public IList<IChangeToken> ExpirationTokens { get; set; } = new List<IChangeToken>();

    public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks { get; set; } = new List<PostEvictionCallbackRegistration>();

    public CacheItemPriority Priority { get; set; }

    public long? Size { get; set; }

    public void Dispose()
    {
    }
}