using System.Threading.Tasks;
using CacheService.Configuration;
using Xunit;

namespace CacheService.Tests.Integration.Configuration
{
    public class When_Memory_RefreshInterval_Is_Not_Set_JobHostedService_Should : ConfigurationIntegrationTestBase
    {
        [Fact]
        public async Task NotUpdateMemoryCache()
        {
            var expected = await Target.GetOrSetAsync(Key, () => new DummyObject(), CancellationToken);
            var actual = await TestActAsync();

            Assert.Equal(expected, actual);
        }

        protected override void OnConfigure(CacheServiceConfiguration configuration)
        {
            base.OnConfigure(configuration);
            configuration.DefaultOption.Memory.RefreshInterval = default;
        }
    }
}
