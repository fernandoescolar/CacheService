using System.Text.Json.Serialization;

namespace CacheService.Tests.Doubles
{
    [JsonConverter(typeof(ErrorInSerializeConverter))]
    public class ErrorInSerialize
    {
    }
}