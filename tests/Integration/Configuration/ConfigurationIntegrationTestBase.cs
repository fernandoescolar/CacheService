using System;
using System.Threading.Tasks;
using CacheService.Tests.Doubles;

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
            if (JobHostedService is null)
            {
                throw new ArgumentNullException(nameof(JobHostedService));
            }

            await JobHostedService.StartAsync(CancellationToken);
            await Task.Delay(2500);
            await JobHostedService.StopAsync(CancellationToken);
        }
    }
}
