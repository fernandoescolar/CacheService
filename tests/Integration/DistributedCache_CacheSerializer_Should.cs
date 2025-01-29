namespace CacheService.Tests.Integration;

public class DistributedCache_CacheSerializer_Should : IntegrationTestBase
{
    [Theory]
#pragma warning disable xUnit1012 // Null should only be used for nullable parameters
    [InlineData(default(byte[]))]
#pragma warning restore xUnit1012 // Null should only be used for nullable parameters
    [InlineData(new byte[0])]
    [InlineData(new byte[] { 105, 110, 118, 97, 108, 105, 100, 32, 106, 115, 111, 110 })] // "invalid json"
    public async Task Not_Throw_an_exception_When_distributed_cache_returns_invalid_values(byte[] invalidValue)
    {
        const string someKey = "whatever";

        MemoryCache.Clear();
        DistributedCache.Add(someKey, invalidValue);

        var actual = await Target.GetOrSetAsync(someKey, _ => ValueTask.FromResult<DummyObject?>(default), CancellationToken);

        Assert.Null(actual);
    }

    [Fact]
    public async Task Not_Throw_an_exception_When_object_to_set_in_distributed_cache_is_not_serializable()
    {
        const string someKey = "whatever";
        var expected = new ErrorInSerialize();
        var actual = await Target.GetOrSetAsync(someKey, _ => ValueTask.FromResult<ErrorInSerialize?>(expected), CancellationToken);

        Assert.Empty(DistributedCache);
        Assert.Equal(expected, actual);
    }
}
