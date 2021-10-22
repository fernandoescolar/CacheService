using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CacheService.OverEngineered
{
    public class DistributedChainLink : ChainLink
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions();

        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<DistributedChainLink> _logger;

        public DistributedChainLink(IDistributedCache distributedCache, ILogger<DistributedChainLink> logger)
        {
            _distributedCache = distributedCache;
            _logger = logger;
        }

        protected override async Task<T?> OnGetAsync<T>(ChainContext<T> context) where T: class
        {
            try
            {
                var bytes = await _distributedCache.GetAsync(context.Key, context.CancellationToken);
                if (bytes is not null && bytes.Length > 0)
                {
                    using (var ms = new MemoryStream(bytes))
                    {

                        var result = await JsonSerializer.DeserializeAsync<T>(ms, _jsonSerializerOptions, context.CancellationToken);
                        if (result is not null)
                        {
                            return result;
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Invalid json format found reading from DistributedCache with key: {context.Key} -> {ex}");
            }

            return default;
        }

        protected override async Task OnSetAsync<T>(ChainContext<T> context)
        {
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, context.Value, _jsonSerializerOptions, context.CancellationToken);
            await _distributedCache.SetAsync(context.Key, ms.GetBuffer(), context.CancellationToken);
        }
    }
}
