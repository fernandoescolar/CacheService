

namespace CacheService.Tests.Integration;

public abstract class IntegrationTestBase : IDisposable
{
    private readonly ServiceProvider _sp;
    private readonly IServiceScope _scope;

    private readonly Lazy<ICacheService> _target;
    private readonly Lazy<IHostedService?> _jobHostedService;
    private readonly Lazy<IJobManager?> _jobManager;
    private bool disposed = false;

    protected IntegrationTestBase()
    {
        MemoryCache = new DummyMemoryCache();
        DistributedCache = new DummyDistributedCache();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IMemoryCache>(MemoryCache);
        services.AddSingleton<IDistributedCache>(DistributedCache);
        services.AddCacheService(OnConfigure);

        _sp = services.BuildServiceProvider();
        _scope = _sp.CreateScope();

        _target = new Lazy<ICacheService>(() => _sp.GetRequiredService<ICacheService>());
        _jobHostedService = new Lazy<IHostedService?>(() => _sp.GetService<IHostedService>());
        _jobManager = new Lazy<IJobManager?>(() => _sp.GetService<IJobManager>());
        TestScope = new CancellationTokenSource();
    }

    ~IntegrationTestBase()
    {
        Dispose(false);
    }

    protected ICacheService Target => _target.Value;

    protected IServiceProvider ServiceProvider => _sp;

    protected IHostedService? JobHostedService => _jobHostedService.Value;

    internal IJobManager? JobManager => _jobManager.Value;

    protected DummyMemoryCache MemoryCache { get; private set; }

    protected DummyDistributedCache DistributedCache { get; private set; }

    protected CancellationTokenSource TestScope { get; private set; }

    protected CancellationToken CancellationToken => TestScope.Token;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposed) return;
        if (disposing)
        {
            TestScope?.Cancel();
            TestScope?.Dispose();
            _scope.Dispose();
            _sp.Dispose();
            disposed = true;
        }
    }

    protected virtual void OnConfigure(CacheServiceConfiguration configuration)
    {
        configuration.BackgroundJobInterval = TimeSpan.FromSeconds(1);
    }
}
