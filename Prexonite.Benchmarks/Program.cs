using BenchmarkDotNet.Running;

namespace Prexonite.Benchmarks;

public static class Program
{
    public static void Main(string[] args)
    {
        BenchmarkSwitcher
            .FromAssembly(typeof(Program).Assembly)
            .Run(args, NoiseResistantConfig.Create());
    }
}
