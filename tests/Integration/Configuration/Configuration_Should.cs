namespace CacheService.Tests.Integration.Configuration;

public class Configuration_Should
{
    [Fact]
    public void Register_Memory_ChainLink_When_It_Is_Required()
    {
        var services = new ServiceCollection();
        services.AddCacheService(op => op.UseMemoryCache = true);

        var actual = services.Any(x => x.ServiceType == typeof(Memory) && x.ImplementationType == typeof(Memory));
        Assert.True(actual);
    }

    [Fact]
    public void Register_Distributed_ChainLink_When_It_Is_Required()
    {
        var services = new ServiceCollection();
        services.AddCacheService(op => op.UseDistributedCache = true);

        var actual = services.Any(x => x.ServiceType == typeof(Distributed) && x.ImplementationType == typeof(Distributed));
        Assert.True(actual);
    }

    [Fact]
    public void Register_AddOrUpdateJob_ChainLink_When_It_Is_Required()
    {
        var services = new ServiceCollection();
        services.AddCacheService(op => op.BackgroundJobMode = BackgroundJobMode.HostedService);

        var actual = services.Any(x => x.ServiceType == typeof(AddOrUpdateJob) && x.ImplementationType == typeof(AddOrUpdateJob));
        Assert.True(actual);
    }

    [Fact]
    public void Register_JobHostedService_ChainLink_When_It_Is_Required()
    {
        var services = new ServiceCollection();
        services.AddCacheService(op => op.BackgroundJobMode = BackgroundJobMode.HostedService);

        var actual = services.Any(x => x.ServiceType == typeof(IHostedService));
        Assert.True(actual);
    }
}
