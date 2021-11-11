using System;
using System.Threading.Tasks;
using CacheService.Configuration;
using CacheService.Tests.Doubles;
using Xunit;

namespace CacheService.Tests.Integration.Configuration
{
    public class When_Distributed_RefreshInterval_Is_Set_JobHostedService_Should : ConfigurationIntegrationTestBase
    {
        [Fact]
        public async Task UpdateDistributeCache()
        {
            var expected = await Target.GetOrSetAsync(Key, () => new DummyObject(), CancellationToken);
            MemoryCache.Clear();

            var actual = await TestActAsync();

            Assert.NotEqual(expected, actual);
        }

        protected override void OnConfigure(CacheServiceConfiguration configuration)
        {
            base.OnConfigure(configuration);
            configuration.DefaultOptions.Distributed.RefreshInterval = TimeSpan.FromSeconds(1);
        }
    }
}
