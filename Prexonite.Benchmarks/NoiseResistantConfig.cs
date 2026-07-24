using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using Perfolizer.Mathematics.OutlierDetection;

namespace Prexonite.Benchmarks;

public static class NoiseResistantConfig
{
    public static IConfig Create()
    {
        var job = Job
            .Default.WithId("NoiseResistant-.NET10")
            .WithRuntime(CoreRuntime.Core10_0)
            .WithLaunchCount(3)
            .WithMinIterationCount(15)
            .WithMaxIterationCount(100)
            .WithMaxRelativeError(0.02)
            .WithOutlierMode(OutlierMode.RemoveUpper);

        return ManualConfig
            .Create(DefaultConfig.Instance)
            .AddJob(job)
            .AddDiagnoser(MemoryDiagnoser.Default)
            .AddColumn(
                StatisticColumn.Median,
                StatisticColumn.StdDev,
                StatisticColumn.P95,
                BaselineRatioColumn.RatioMean
            )
            .WithOption(ConfigOptions.StopOnFirstError, true);
    }
}
