using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;

namespace CacheService.Tests.Doubles
{
    internal class DummyMemoryCache : Dictionary<string, DummyCacheEntry>, IMemoryCache
    {
        public ICacheEntry CreateEntry(object key)
        {
            var entry = new DummyCacheEntry(key?.ToString() ?? string.Empty);
            Add(entry.KeyString, entry);

            return entry;
        }

        public void Dispose()
        {
        }

        public void Remove(object key)
        {
            base.Remove(key?.ToString() ?? string.Empty);
        }

        public bool TryGetValue(object key, out object? value)
        {
            var k = key?.ToString() ?? string.Empty;
            if (base.ContainsKey(k))
            { 
                value = base[k];
                return true;
            }

            value = default;
            return false;
        }
    }
}
