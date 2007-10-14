using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Prexonite;

namespace Prx.Benchmarking
{
    public sealed class BenchmarkEntry
    {
        #region Features

        public readonly PFunction Function;
        public readonly string Title;
        public readonly string Description;
        public readonly bool UsesIteration;
        public readonly string Overhead;
        public readonly Benchmark Parent;

        public List<Measurement> Measurements
        {
            get { return _measurements; }
        }

        private List<Measurement> _measurements = new List<Measurement>();

        public long GetAverageRawMilliseconds()
        {
            ulong sum = 0;
            foreach (Measurement m in _measurements)
                sum += (ulong)m.RawMilliseconds;
            return (long) Math.Round(((double)sum/(double)_measurements.Count),MidpointRounding.AwayFromZero);
        }

        internal BenchmarkEntry(Benchmark parent, PFunction function)
        {
            if (function == null)
                throw new ArgumentNullException("function");
            if (parent == null)
                throw new ArgumentNullException("parent");
            Parent = parent;
            Function = function;
            MetaTable m = function.Meta;
            if (m.ContainsKey(Benchmark.TitleKey))
                Title = m[Benchmark.TitleKey];
            else
                Title = function.Id;

            if (m.ContainsKey(Benchmark.DescriptionKey))
                Description = m[Benchmark.DescriptionKey];
            else
                Description = String.Format("The benchmarked function {0}", Title);

            UsesIteration = m[Benchmark.UsesIterationKey];

            if (m.ContainsKey(Benchmark.OverheadKey))
                Overhead = m[Benchmark.OverheadKey];
        }

        #endregion

        #region Equality

        public override bool Equals(object obj)
        {
            if(obj == null)
                return false;
            BenchmarkEntry be = obj as BenchmarkEntry;
            if (be != null)
                return Function.Equals(be.Function);
            else
                return false;
        }

        public override int GetHashCode()
        {
            return Function.GetHashCode() ^ 12;
        }

        public static bool operator ==(BenchmarkEntry be1, BenchmarkEntry be2)
        {
            if ((object)be1 == null && (object)be2 == null)
                return true;
            else if ((object)be1 == null || (object)be2 == null)
                return false;
            else
                return be1.Equals(be2);
        }

        public static bool operator !=(BenchmarkEntry be1, BenchmarkEntry be2)
        {
            if ((object)be1 == null && (object)be2 == null)
                return false;
            else if ((object)be1 == null || (object)be2 == null)
                return true;
            else
                return !(be1.Equals(be2));
        }

        #endregion

        #region Measurement

        public Measurement Measure(bool verbose)
        {
            int iterations = Parent.Iterations;
            Stopwatch sw = Parent._Stopwatch;
            if (verbose)
            {
                Console.WriteLine(
                    "--------------------------------------\n{0}\n {1}", Title, Description);
                if(UsesIteration)
                    Console.WriteLine("\tIterations:\t{0}",iterations);
            }

            FunctionContext fctx =
                    Function.CreateFunctionContext(Parent.Machine, new PValue[] { iterations });
            sw.Reset();
            sw.Start();
            Parent.Machine.Process(fctx);
            sw.Stop();

            long raw = sw.ElapsedMilliseconds;
            long overhead = 0;
            if (Overhead != null && Parent.Entries.Contains(Overhead))
                overhead = Parent.Entries[Overhead].GetAverageRawMilliseconds();

            Measurement m = new Measurement(this, raw, overhead);

            if(verbose)
            {
                Console.WriteLine(
                    "\tmeasured:\t{0} ms\n" +
                    "\t\t\t{1:0.00} s\n" +
                    "\tpass:\t\t{2:0.00} ms\n" +
                    "\t\t\t{3:0.00} micros",
                    m.RawMilliseconds,
                    m.RawSeconds,
                    m.PassMilliseconds,
                    m.PassMicroseconds);
                if(overhead > 0)
                    Console.WriteLine(
                        "\tcleared:\t{0:0.00} ms\n" +
                        "\t\t\t{1:0.00} micros",
                        m.ClearedPassMilliseconds,
                        m.ClearedPassMicroseconds);
            }

            return m;
        }

        #endregion

        public void WarmUp()
        {
            FunctionContext fctx =
                Function.CreateFunctionContext(
                    Parent.Machine, new PValue[] {Benchmark.DefaultWarmUpIterations});
            Parent.Machine.Process(fctx);
        }
    }
}
