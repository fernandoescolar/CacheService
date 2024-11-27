namespace CacheService.Tests.Integration.Configuration;

public class Configuration_Should
{
    [Fact]
    public void Register_JobHostedService_ChainLink_When_It_Is_Required()
    {
        var services = new ServiceCollection();
        services.AddCacheService(op => op.BackgroundJobMode = BackgroundJobMode.HostedService);

        var actual = services.Any(x => x.ServiceType == typeof(IHostedService));
        Assert.True(actual);
    }
}
