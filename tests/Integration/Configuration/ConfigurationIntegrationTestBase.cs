using System;
using System.Threading.Tasks;
using CacheService.Configuration;

namespace CacheService.Tests.Integration.Configuration
{
    public abstract class ConfigurationIntegrationTestBase : IntegrationTestBase
    {
        protected ConfigurationIntegrationTestBase()
        {
            Key = Guid.NewGuid().ToString();
        }

        protected string Key { get; private set; }

        protected async Task<DummyObject?> TestActAsync()
        {
            await RunJobHostedServiceAsync();

            return await Target.GetOrSetAsync(Key, () => new DummyObject(), CancellationToken);
        }

        protected async Task RunJobHostedServiceAsync()
        {
            await JobHostedService.StartAsync(CancellationToken);
            await Task.Delay(2500);
            await JobHostedService.StopAsync(CancellationToken);
        }
    }
}
