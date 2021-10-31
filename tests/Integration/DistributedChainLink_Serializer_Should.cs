using CacheService.ChainLinks;
using CacheService.Core;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CacheService.Tests.Integration
{
    public class DistributedChainLink_CacheSerializer_Should : IntegrationTestBase
    {
        private readonly Distributed target;

        public DistributedChainLink_CacheSerializer_Should()
            : base()
        {
            target = ServiceProvider.GetRequiredService<IEnumerable<IChainLink>>().OfType<Distributed>().Single();
        }

        [Theory]
        [InlineData(default(byte[]))]
        [InlineData(new byte[0])]
        [InlineData(new byte[] { 105, 110, 118, 97, 108, 105, 100, 32, 106, 115, 111, 110 })] // "invalid json"
        public async Task Not_Throw_an_exception_When_distributed_cache_returns_invalid_values(byte[] invalidValue)
        {
            const string someKey = "whatever";
            var someDummyObjectGetter = (CancellationToken ct) => ValueTask.FromResult(new DummyObject());


            DistributedCache.Add(someKey, invalidValue);

            var context = new ChainContext<DummyObject>(someKey, CacheServiceOptions.Default, someDummyObjectGetter, CancellationToken);
            var actual = await target.HandleAsync(context);

            Assert.Null(actual);
        }

        [Fact]
        public async Task Not_Throw_an_exception_When_object_to_set_in_distributed_cache_is_not_serializable()
        {
            const string someKey = "whatever";
            var expected = new ErrorInSerialize();
            var context = new ChainContext<ErrorInSerialize>(someKey, CacheServiceOptions.Default, ct => ValueTask.FromResult<ErrorInSerialize?>(default), CancellationToken);
            target.Next = new DummyChainLink(expected);


            var actual = await target.HandleAsync(context);

            Assert.Empty(DistributedCache);
            Assert.Equal(expected, actual);
        }
    }
}
