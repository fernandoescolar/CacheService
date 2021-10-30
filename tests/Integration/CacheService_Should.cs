using System;
using System.Threading.Tasks;
using Xunit;

namespace CacheService.Tests.Integration
{
    public class CacheService_Should : IntegrationTestBase
    {
        private readonly string key;
        private readonly DummyObject expected;
        private readonly byte[] serialized;

        public CacheService_Should()
            : base()
        {
            key = Guid.NewGuid().ToString();
            expected = new();
            serialized = System.Text.Encoding.UTF8.GetBytes($@"{{""Id"":""{expected.Id}""}}");
        }

        [Fact]
        public async Task ReadFromMemoryCache()
        {
            MemoryCache.Add(key, new DummyCacheEntry(key) { Value = expected });

            var actual = await Target.GetOrSetAsync(key, () => new DummyObject(), CancellationToken);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task ReadFromDistributedCache()
        {
            DistributedCache.Add(key, serialized);

            var actual = await Target.GetOrSetAsync(key, () => new DummyObject(), CancellationToken);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task ReadFromValueGetter()
        {
            var actual = await Target.GetOrSetAsync(key, () => expected, CancellationToken);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task WriteMemoryCacheWhenValueIsReadDistributedCache()
        {
            DistributedCache.Add(key, serialized);

            var actual = await Target.GetOrSetAsync(key, () => new DummyObject(), CancellationToken);
            var memoryValue = MemoryCache[key].Value;

            Assert.Equal(expected, actual);
            Assert.Equal(expected, memoryValue);
        }

        [Fact]
        public async Task WriteMemoryCacheWhenValueIsReadFromValueGetter()
        {
            var actual = await Target.GetOrSetAsync(key, () => expected, CancellationToken);
            var memoryValue = MemoryCache[key].Value;

            Assert.Equal(expected, actual);
            Assert.Equal(expected, memoryValue);
        }

        [Fact]
        public async Task WriteDistributedCacheWhenValueIsReadFromValueGetter()
        {
            var actual = await Target.GetOrSetAsync(key, () => expected, CancellationToken);
            var distributedValue = DistributedCache[key];

            Assert.Equal(serialized, distributedValue);
        }
    }
}
