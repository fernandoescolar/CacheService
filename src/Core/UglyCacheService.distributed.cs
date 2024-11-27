namespace CacheService.Core;

sealed partial class UglyCacheService
{
   private async ValueTask<T?> TryGetFromDistributedAsync<T>(string key, CacheServiceOptions options, CancellationToken cancellationToken) where T : class
    {
        if (_useDistributedCache)
        {
            try
            {
                #nullable disable
                var bytes = await _distributedCache.GetAsync(key, cancellationToken);
                #nullable restore

                if (bytes is not null && bytes.Length > 0)
                {
                    T? result;
                    if (_useCustomSerializer)
                    {
                        #nullable disable
                        result = await _serializer.DeserializeAsync<T>(bytes, cancellationToken) ?? default;
                        #nullable restore
                    }
                    else
                    {
                        result = FastJsonSerializer.Deserialize<T>(bytes);
                    }

                    if (result is not null)
                    {
                        TrySetMemory(key, options, result);
                        return result;
                    }
                }
            }
            catch (JsonException jex)
            {
                _logger.CannotDeserializeJson(key, jex.Message);
            }
            catch (Exception ex)
            {
                _logger.CannotGetDistributedCache(key, ex.Message);
            }
        }

        return default;
    }

    private void TrySetDistributed<T>(string key, CacheServiceOptions ops, T? result) where T : class
    {
        if (_useDistributedCache)
        {
            Fire.Forget(async () =>
            {
                try
                {
                    byte[] bytes;
                    if (_useCustomSerializer)
                    {
                        #nullable disable
                        bytes = await _serializer.SerializeAsync(result, default) ?? [];
                        #nullable restore
                    }
                    else
                    {
                        bytes = FastJsonSerializer.Serialize(result);
                    }

                    #nullable disable
                    await _distributedCache.SetAsync(key, bytes, ops.Distributed, default);
                    #nullable restore
                }
                catch (JsonException jex)
                {
                    _logger.CannotSerializeJson(key, jex.Message);
                }
                catch (OutOfMemoryException mex)
                {
                    _logger.OutOfMemory(key, mex.Message);
                }
                catch (Exception ex)
                {
                    _logger.CannotSetDistributedCache(key, ex.Message);
                }
            });
        }
    }
}
