namespace CacheService.ChainLinks;

internal class Distributed : ChainLink
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

    protected override async ValueTask<T?> OnGetAsync<T>(ChainContext<T> context) where T: class
    {
        try
        {
            var bytes = await _distributedCache.GetAsync(context.Key, context.CancellationToken);
            if (bytes is not null && bytes.Length > 0)
            {
                return await _serializer.DeserializeAsync<T>(bytes, context.CancellationToken);
            }
        }
        catch(JsonException jex)
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
        try
        {
            if (IsInvalidateAction(context))
            {
                await _distributedCache.RemoveAsync(context.Key, context.CancellationToken);
                return;
            }

            var bytes = await _serializer.SerializeAsync(context.Value, context.CancellationToken) ?? Array.Empty<byte>();
            await _distributedCache.SetAsync(context.Key, bytes, context.Options.Distributed, context.CancellationToken);
        }
        catch(JsonException jex)
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
}