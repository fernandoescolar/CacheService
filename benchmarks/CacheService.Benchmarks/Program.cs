using BenchmarkDotNet.Configs;

Console.WriteLine("Benchmark running...");

//BenchmarkRunner.Run(typeof(CacheServiceVsHybridCache), new DebugInProcessConfig());
BenchmarkRunner.Run(typeof(CacheServiceVsHybridCache));

