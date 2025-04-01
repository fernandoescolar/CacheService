

namespace CacheService.Tests.Integration;

public class JobManager_Should_Not_Raise_Exceptions : IntegrationTestBase
{
    private readonly string key;
    private readonly CacheServiceOptions options;

    public JobManager_Should_Not_Raise_Exceptions()
    {
        key = Guid.NewGuid().ToString();
        options = new CacheServiceOptions();
    }

    protected override void OnConfigure(CacheServiceConfiguration configuration)
    {
        base.OnConfigure(configuration);
        configuration.BackgroundJobIgnoreExceptions = true;
    }

    [Fact]
    public async Task UpdateCache()
    {
        options.Memory.RefreshInterval = TimeSpan.FromSeconds(1);
        try
        {
            await Target.GetOrSetAsync(key, options, async ct => await GetExceptionAsync(), CancellationToken);
        }
        catch (Exception ex)
        {
            Assert.IsType<Exception>(ex);
            Assert.Equal("Test exception", ex.Message);
        }


        await RunJobHostedServiceAsync();
    }

    private async Task RunJobHostedServiceAsync()
    {
        if (JobManager is null)
        {
            throw new InvalidOperationException("JobManager is not initialized.");
        }

        await Task.Delay(2500);
        await JobManager.ExecuteJobsAsync(CancellationToken);
    }

    private static Task<DummyObject?> GetExceptionAsync()
    {
        throw new Exception("Test exception");
    }
}
