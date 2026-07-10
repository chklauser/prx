using System;
using System.Collections.Generic;
using System.Linq;
using Prexonite;

namespace Prx.Benchmarking;

public sealed class BenchmarkEntry
{
    #region Features

    public readonly PFunction Function;
    public readonly string Title;
    public readonly string Description;
    public readonly bool UsesIteration;
    public readonly string? Overhead;
    public readonly Benchmark Parent;

    public List<Measurement> Measurements { get; } = [];

    public long GetAverageRawMilliseconds()
    {
        var sum = Measurements.Aggregate<Measurement, ulong>(
            0,
            (current, m) => current + (ulong)m.RawMilliseconds
        );
        return (long)Math.Round(sum / (double)Measurements.Count, MidpointRounding.AwayFromZero);
    }

    internal BenchmarkEntry(Benchmark parent, PFunction function)
    {
        Parent = parent ?? throw new ArgumentNullException(nameof(parent));
        Function = function ?? throw new ArgumentNullException(nameof(function));
        var m = function.Meta;
        if (m.TryGetValue(Benchmark.TitleKey, out var value))
            Title = value;
        else
            Title = function.Id;

        if (m.TryGetValue(Benchmark.DescriptionKey, out var value1))
            Description = value1;
        else
            Description = $"The benchmarked function {Title}";

        UsesIteration = m[Benchmark.UsesIterationKey];

        if (m.TryGetValue(Benchmark.OverheadKey, out var value2))
            Overhead = value2;
    }

    #endregion

    #region Equality

    public override bool Equals(object? obj)
    {
        if (obj == null)
            return false;
        var be = obj as BenchmarkEntry;
        return be != null && Function.Equals(be.Function);
    }

    public override int GetHashCode()
    {
        return Function.GetHashCode() ^ 12;
    }

    public static bool operator ==(BenchmarkEntry? be1, BenchmarkEntry? be2)
    {
        if ((object?)be1 == null && (object?)be2 == null)
            return true;
        else if ((object?)be1 == null || (object?)be2 == null)
            return false;
        else
            return be1.Equals(be2);
    }

    public static bool operator !=(BenchmarkEntry? be1, BenchmarkEntry? be2)
    {
        if ((object?)be1 == null && (object?)be2 == null)
            return false;
        else if ((object?)be1 == null || (object?)be2 == null)
            return true;
        else
            return !be1.Equals(be2);
    }

    #endregion

    #region Measurement

    public Measurement Measure(bool verbose)
    {
        var iterations = Parent.Iterations;
        var sw = Parent._Stopwatch;
        if (verbose)
        {
            Console.WriteLine(
                "--------------------------------------\n{0}\n {1}",
                Title,
                Description
            );
            if (UsesIteration)
                Console.WriteLine("\tIterations:\t{0}", iterations);
        }

        sw.Reset();
        sw.Start();
        Function.Run(Parent.Machine, [iterations]);
        sw.Stop();

        var raw = sw.ElapsedMilliseconds;
        long overhead = 0;
        if (Overhead != null && Parent.Entries.Contains(Overhead))
            overhead = Parent.Entries[Overhead].GetAverageRawMilliseconds();

        var m = new Measurement(this, raw, overhead);

        if (verbose)
        {
            Console.WriteLine(
                "\tmeasured:\t{0} ms\n"
                    + "\t\t\t{1:0.00} s\n"
                    + "\tpass:\t\t{2:0.00} ms\n"
                    + "\t\t\t{3:0.00} micros",
                m.RawMilliseconds,
                m.RawSeconds,
                m.PassMilliseconds,
                m.PassMicroseconds
            );
            if (overhead > 0)
                Console.WriteLine(
                    "\tcleared:\t{0:0.00} ms\n" + "\t\t\t{1:0.00} micros",
                    m.ClearedPassMilliseconds,
                    m.ClearedPassMicroseconds
                );
        }

        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);

        return m;
    }

    #endregion

    public void WarmUp()
    {
        var fctx = Function.CreateFunctionContext(
            Parent.Machine,
            [Benchmark.DefaultWarmUpIterations]
        );
        Parent.Machine.Process(fctx);
    }
}
