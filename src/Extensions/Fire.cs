namespace CacheService.Extensions;

internal static class Fire
{
    public static void Forget(Func<Task> task)
        => _ = Task.Run(async () => await task());
}
