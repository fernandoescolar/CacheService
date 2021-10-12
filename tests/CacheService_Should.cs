using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CacheService.Tests
{
    public record DummyObject();
    public class DummyCacheEntry : ICacheEntry
    {
        public object? Key { get; set; }

        public object? Value { get; set; }
        public DateTimeOffset? AbsoluteExpiration { get; set; }
        public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }
        public TimeSpan? SlidingExpiration { get; set; }

        public IList<IChangeToken>? ExpirationTokens { get; set; }

        public IList<PostEvictionCallbackRegistration>? PostEvictionCallbacks { get; set; }

        public CacheItemPriority Priority { get; set; }
        public long? Size { get; set; }

        public void Dispose()
        {
        }
    }

    public class CacheService_Should : IDisposable
    {
        private readonly Mock<IMemoryCache> memoryCache;
        private readonly Mock<IDistributedCache> distributedCache;
        private readonly ILogger<DefaultCacheService> logger = NullLogger<DefaultCacheService>.Instance;
        private readonly ICacheService target;
        private readonly CancellationTokenSource testScope;

        public CacheService_Should()
        {
            memoryCache = new Mock<IMemoryCache>();
            distributedCache = new Mock<IDistributedCache>();
            target = new DefaultCacheService(memoryCache.Object, distributedCache.Object, logger);
            testScope = new CancellationTokenSource();
        }

        [Theory]
        [InlineData(default(byte[]))]
        [InlineData(new byte[0])]
        [InlineData(new byte[] { 105, 110, 118, 97, 108, 105, 100, 32, 106, 115, 111, 110 })] // "invalid json"
        public async Task Not_Throw_an_exception_When_distributed_cache_returns_invalid_values(byte[] invalidValue)
        {
            const string someKey = "whatever";
            var expected = new DummyObject();
            var someDummyObjectGetter = () => expected;
            object o;
                
            memoryCache.Setup(x => x.TryGetValue(someKey, out o))
                       .Returns(false);

            memoryCache.Setup(x => x.CreateEntry(someKey))
                       .Returns(new DummyCacheEntry());

            distributedCache.Setup(x => x.GetAsync(someKey, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(invalidValue);

            var actual = await target.GetOrSetAsync<DummyObject>(someKey, someDummyObjectGetter, testScope.Token);
            Assert.Equal(expected, actual);
        }

        public void Dispose()
        {
            testScope?.Cancel();
            testScope?.Dispose();
        }
    }
}