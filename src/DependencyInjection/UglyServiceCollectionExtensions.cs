namespace Microsoft.Extensions.DependencyInjection;

public static class UglyServiceCollectionExtensions
{
    public static IServiceCollection AddUglyCacheService(this IServiceCollection services, Action<CacheServiceConfiguration>? configure = null)
    {
        services.Configure<CacheServiceConfiguration>(op => configure?.Invoke(op));
        services.TryAddSingleton<UglyCacheService>();

        return services;
    }
}
