using CacheService.Configuration;
using CacheService.Tests.Doubles;
using System;
using System.Threading.Tasks;
using Xunit;

namespace CacheService.Tests.Integration
{
    public class JobTimer_Should : IntegrationTestBase
    {
        private readonly string key;
        private readonly CacheServiceOptions options;

        public JobTimer_Should()
        {
            key = Guid.NewGuid().ToString();
            options = new CacheServiceOptions();
        }

        [Fact]
        public async Task Update_MemoryCache()
        {
            options.Memory.RefreshInterval = TimeSpan.FromSeconds(1);

            var expected = await Target.GetOrSetAsync(key, options, () => new DummyObject(), CancellationToken);
            var actual = await TestActAsync();

            Assert.NotEqual(expected, actual);
        }

        [Fact]
        public async Task Update_DistributedCache()
        {
            options.Distributed.RefreshInterval = TimeSpan.FromSeconds(1);

            var expected = await Target.GetOrSetAsync(key, options, () => new DummyObject(), CancellationToken);
            MemoryCache.Clear();

            var actual = await TestActAsync();

            Assert.NotEqual(expected, actual);
        }

        [Fact]
        public async Task Not_Update_MemoryCache()
        {
            options.Memory.RefreshInterval = default;

            var expected = await Target.GetOrSetAsync(key, options, () => new DummyObject(), CancellationToken);
            var actual = await TestActAsync();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task Not_Update_DistributedCache()
        {
            options.Distributed.RefreshInterval = default;

            var expected = await Target.GetOrSetAsync(key, options, () => new DummyObject(), CancellationToken);
            MemoryCache.Clear();

            var actual = await TestActAsync();

            Assert.Equal(expected, actual);
        }

        protected override void OnConfigure(CacheServiceConfiguration configuration)
        {
            base.OnConfigure(configuration);
            configuration.BackgroundJobMode = BackgroundJobMode.Timer;
        }

        private async Task<DummyObject?> TestActAsync()
        {
            await WaitTimerDoHisJob();

            return await Target.GetOrSetAsync(key, options, () => new DummyObject(), CancellationToken);
        }

        private static Task WaitTimerDoHisJob()
            => Task.Delay(2500);
    }
}
