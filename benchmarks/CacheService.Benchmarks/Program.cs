Console.WriteLine("Benchmark running...");

if (args.Length == 0)
{
    // default: CacheService vs HybridCache (needs a Redis instance in localhost:6379)
    BenchmarkRunner.Run<CacheServiceVsHybridCache>();
}
else
{
    // e.g.: dotnet run -c Release -- --filter *HotPaths*
    BenchmarkSwitcher.FromAssembly(typeof(CacheServiceVsHybridCache).Assembly).Run(args);
}
