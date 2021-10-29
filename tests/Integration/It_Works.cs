using CacheService.Tests.Doubles;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;

namespace CacheService.Tests.Integration
{
    public class It_Works : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly ICacheService _target;
        private readonly ICacheSerializer _serializer;
        private readonly DummyMemoryCache _memory;
        private readonly DummyDistributedCache _distributed;

        public It_Works()
        {
            _memory = new DummyMemoryCache();
            _distributed = new DummyDistributedCache();

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<IMemoryCache>(_memory);
            services.AddSingleton<IDistributedCache>(_distributed);
            services.AddCacheService();

            _serviceProvider = services.BuildServiceProvider();
            _target = _serviceProvider.GetRequiredService<ICacheService>();
            _serializer = _serviceProvider.GetRequiredService<ICacheSerializer>();
        }

        [Fact]
        public async Task Test_It()
        {
            const string key = "ola k ase";
            var expected = new DummyObject();

            await _target.GetOrSetAsync(key, () => expected);

            var actualMemory = _memory[key].Value;
            var actualDistributed = _distributed[key];

            Assert.Equal(actualMemory, expected);
            Assert.NotNull(actualDistributed);
        }


        [Fact]
        public async Task Test_It_Distributed_Only()
        {
            const string key = "ola k ase";
            var expected = new DummyObject();
            var unexpected = new DummyObject();
            var serialized = await _serializer.SerializeAsync(expected, default);
            _distributed[key] = serialized;

            await _target.GetOrSetAsync(key, () => unexpected);

            var actual = _memory[key].Value;

            Assert.Equal(actual, expected);
        }

        public void Dispose()
        {
            _serviceProvider.Dispose();
        }
    }
}
