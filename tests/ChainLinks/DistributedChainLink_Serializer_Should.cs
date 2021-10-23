using CacheService.ChainLinks;
using CacheService.Core;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CacheService.Tests.ChainLinks
{
    public class ErrorInSerializeConverter : JsonConverter<ErrorInSerialize>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            throw new NotImplementedException();
        }

        public override ErrorInSerialize? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, ErrorInSerialize value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }

    [JsonConverter(typeof(ErrorInSerializeConverter))]
    public class ErrorInSerialize
    { 
    }

    public class DummyChainLink : ChainLink 
    {
        private readonly object? _value = default;

        public DummyChainLink()
        {
        }

        public DummyChainLink(object value)
        {
            _value = value;
        }

        protected override ValueTask<T?> OnGetAsync<T>(ChainContext<T> context) where T : class
            => ValueTask.FromResult(_value as T);
    }

    public class DistributedChainLink_Serializer_Should : IDisposable
    {
        private readonly ILogger<Distributed> logger;
        private readonly Mock<IDistributedCache> distributedCache;
        private readonly Distributed target;
        private readonly CancellationTokenSource testScope;

        public DistributedChainLink_Serializer_Should()
        {
            logger = NullLogger<Distributed>.Instance;
            distributedCache = new Mock<IDistributedCache>();
            target = new Distributed(distributedCache.Object, logger);
            testScope = new CancellationTokenSource();
        }

        public void Dispose()
        {
            testScope?.Cancel();
            testScope?.Dispose();
        }

        [Theory]
        [InlineData(default(byte[]))]
        [InlineData(new byte[0])]
        [InlineData(new byte[] { 105, 110, 118, 97, 108, 105, 100, 32, 106, 115, 111, 110 })] // "invalid json"
        public async Task Not_Throw_an_exception_When_distributed_cache_returns_invalid_values(byte[] invalidValue)
        {
            const string someKey = "whatever";
            var someDummyObjectGetter = (CancellationToken ct) => ValueTask.FromResult(new DummyObject()); 


            distributedCache.Setup(x => x.GetAsync(someKey, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(invalidValue);

            var context = new ChainContext<DummyObject>(someKey, CacheServiceOptions.Default, someDummyObjectGetter, testScope.Token);
            var actual = await target.HandleAsync(context);

            Assert.Null(actual);
        }

        [Fact]
        public async Task Not_Throw_an_exception_When_object_to_set_in_distributed_cache_is_not_serializable()
        {
            const string someKey = "whatever";
            var expected = new ErrorInSerialize();

            distributedCache.Setup(x => x.GetAsync(someKey, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(default(byte[]));

            target.Next = new DummyChainLink(expected);

            var context = new ChainContext<ErrorInSerialize>(someKey, CacheServiceOptions.Default, null, testScope.Token);
            var actual = await target.HandleAsync(context);

            Assert.Equal(expected, actual);
            distributedCache.Verify(x => x.SetAsync(someKey, It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
