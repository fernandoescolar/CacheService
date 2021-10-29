using System.Text.Json.Serialization;

namespace CacheService.Tests
{
    [JsonConverter(typeof(ErrorInSerializeConverter))]
    public class ErrorInSerialize
    {
    }
}