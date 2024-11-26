namespace CacheService.Tests.Doubles;

[JsonConverter(typeof(ErrorInSerializeConverter))]
public class ErrorInSerialize
{
}