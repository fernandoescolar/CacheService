namespace CacheService.ChainLinks;

internal sealed class Distributed : ChainLink
{
    private readonly IDistributedCache _distributedCache;
    private readonly ICacheSerializer _serializer;
    private readonly ILogger<Distributed> _logger;

    public Distributed(IDistributedCache distributedCache, ICacheSerializer serializer, ILogger<Distributed> logger)
        : base(20)
    {
        _distributedCache = distributedCache;
        _serializer = serializer;
        _logger = logger;
    }

    protected override async ValueTask<T?> OnGetAsync<T>(ChainContext<T> context) where T : class
    {
        try
        {
            var bytes = await _distributedCache.GetAsync(context.Key, context.CancellationToken);
            if (bytes is not null && bytes.Length > 0)
            {
                if (_serializer is EmptyCacheSerializer)
                {
                    return FastJsonSerializer.Deserialize<T>(bytes);
                }

                return await _serializer.DeserializeAsync<T>(bytes, context.CancellationToken);
            }
        }
        catch (JsonException jex)
        {
            _logger.CannotDeserializeJson(context.Key, jex.Message);
        }
        catch (Exception ex)
        {
            _logger.CannotSet(context.Key, ex.Message);
        }

        return default;
    }

    protected override async Task OnSetAsync<T>(ChainContext<T> context)
    {
        if (IsInvalidateAction(context))
        {
            await InvalidateValue(context);
        }
        else
        {
            Fire.Forget(() => UpdateValue(context));
        }
    }

    private async Task InvalidateValue<T>(ChainContext<T> context)
    {
        try
        {
            await _distributedCache.RemoveAsync(context.Key, context.CancellationToken);
        }
        catch (Exception ex)
        {
            _logger.CannotInvalidate(context.Key, ex.Message);
        }
    }

    private async Task UpdateValue<T>(ChainContext<T> context)
    {
        try
        {
            byte[] bytes;
            if (_serializer is EmptyCacheSerializer)
            {
                bytes = FastJsonSerializer.Serialize(context.Value);
            }
            else
            {
                bytes = await _serializer.SerializeAsync(context.Value, context.CancellationToken) ?? [];
            }

            await _distributedCache.SetAsync(context.Key, bytes, context.Options.Distributed, context.CancellationToken);
        }
        catch (JsonException jex)
        {
            _logger.CannotDeserializeJson(context.Key, jex.Message);
        }
        catch (Exception ex)
        {
            _logger.CannotSet(context.Key, ex.Message);
        }
    }

    private static bool IsInvalidateAction<T>(ChainContext<T> context)
        => context.Options.ForceRefresh
        && context.Options.Distributed.AbsoluteExpiration <= DateTimeOffset.UtcNow
        && context.Value is null;
}

internal static partial class DistributedLoggerExtensions
{
    [LoggerMessage(0, LogLevel.Warning, "Cannot deserialize from json in DistributedCache with key: {Key} -> {jex}")]
    public static partial void CannotDeserializeJson(this ILogger logger, string key, string jex);

    [LoggerMessage(1, LogLevel.Warning, "Cannot set to DistributedCache with key: {Key} -> {ex}")]
    public static partial void CannotSet(this ILogger logger, string key, string ex);

     [LoggerMessage(2, LogLevel.Warning, "Cannot invalidate to DistributedCache with key: {Key} -> {ex}")]
    public static partial void CannotInvalidate(this ILogger logger, string key, string ex);
}