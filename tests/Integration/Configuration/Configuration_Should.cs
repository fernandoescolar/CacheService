using System.Linq;
using CacheService.ChainLinks;
using CacheService.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace CacheService.Tests.Integration.Configuration
{
    public class Configuration_Should
    {
        [Fact]
        public void Register_Memory_ChainLink_When_It_Is_Required()
        {
            var services = new ServiceCollection();
            services.AddCacheService(op => op.UseMemoryCache = true);

            var actual = services.Any(x => x.ServiceType == typeof(IChainLink) && x.ImplementationType == typeof(Memory));
            Assert.True(actual);
        }

        [Fact]
        public void Not_Register_Memory_ChainLink_When_It_Is_Required()
        {
            var services = new ServiceCollection();
            services.AddCacheService(op => op.UseMemoryCache = false);

            var actual = services.Any(x => x.ServiceType == typeof(IChainLink) && x.ImplementationType == typeof(Memory));
            Assert.False(actual);
        }

        [Fact]
        public void Register_Distributed_ChainLink_When_It_Is_Required()
        {
            var services = new ServiceCollection();
            services.AddCacheService(op => op.UseDistributedCache = true);

            var actual = services.Any(x => x.ServiceType == typeof(IChainLink) && x.ImplementationType == typeof(Distributed));
            Assert.True(actual);
        }

        [Fact]
        public void Not_Register_Distributed_ChainLink_When_It_Is_Required()
        {
            var services = new ServiceCollection();
            services.AddCacheService(op => op.UseDistributedCache = false);

            var actual = services.Any(x => x.ServiceType == typeof(IChainLink) && x.ImplementationType == typeof(Distributed));
            Assert.False(actual);
        }

        [Fact]
        public void Register_AddOrUpdateJob_ChainLink_When_It_Is_Required()
        {
            var services = new ServiceCollection();
            services.AddCacheService(op => op.UseJobHostedService = true);

            var actual = services.Any(x => x.ServiceType == typeof(IChainLink) && x.ImplementationType == typeof(AddOrUpdateJob));
            Assert.True(actual);
        }

        [Fact]
        public void Not_Register_AddOrUpdateJob_ChainLink_When_It_Is_Required()
        {
            var services = new ServiceCollection();
            services.AddCacheService(op => op.UseJobHostedService = false);

            var actual = services.Any(x => x.ServiceType == typeof(IChainLink) && x.ImplementationType == typeof(AddOrUpdateJob));
            Assert.False(actual);
        }

        [Fact]
        public void Register_JobHostedService_ChainLink_When_It_Is_Required()
        {
            var services = new ServiceCollection();
            services.AddCacheService(op => op.UseJobHostedService = true);

            var actual = services.Any(x => x.ServiceType == typeof(IHostedService));
            Assert.True(actual);
        }

        [Fact]
        public void Not_Register_JobHostedService_ChainLink_When_It_Is_Required()
        {
            var services = new ServiceCollection();
            services.AddCacheService(op => op.UseJobHostedService = false);

            var actual = services.Any(x => x.ServiceType == typeof(IHostedService));
            Assert.False(actual);
        }
    }
}
