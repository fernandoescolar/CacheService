using CacheService.Core;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CacheService.ChainLinks
{
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
                _logger.LogWarning($"Cannot deserialize from json in DistributedCache with key: {context.Key} -> {jex}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Cannot get from DistributedCache with key: {context.Key} -> {ex}");
            }

            return default;
        }

        protected override async Task OnSetAsync<T>(ChainContext<T> context)
        {
            try
            {
                var bytes = await _serializer.SerializeAsync(context.Value, context.CancellationToken);
                await _distributedCache.SetAsync(context.Key, bytes, context.Options.Distributed, context.CancellationToken);
            }
            catch(JsonException jex)
            {
                _logger.LogWarning($"Cannot serialize to json in DistributedCache with key: {context.Key} -> {jex}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Cannot set to DistributedCache with key: {context.Key} -> {ex}");
            }
        }
    }
}
