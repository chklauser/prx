// Prexonite
// 
// Copyright (c) 2014, Christian Klauser
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, 
//  are permitted provided that the following conditions are met:
// 
//     Redistributions of source code must retain the above copyright notice, 
//          this list of conditions and the following disclaimer.
//     Redistributions in binary form must reproduce the above copyright notice, 
//          this list of conditions and the following disclaimer in the 
//          documentation and/or other materials provided with the distribution.
//     The names of the contributors may be used to endorse or 
//          promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
//  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
//  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
//  IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
//  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
//  (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
//  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
//  WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING 
//  IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
using System;
using System.Collections.Generic;
using System.Linq;
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

        private readonly List<Measurement> _measurements = new List<Measurement>();

        public long GetAverageRawMilliseconds()
        {
            var sum = _measurements.Aggregate<Measurement, ulong>(0,
                (current, m) => current + (ulong) m.RawMilliseconds);
            return
                (long) Math.Round((sum/(double) _measurements.Count), MidpointRounding.AwayFromZero);
        }

        internal BenchmarkEntry(Benchmark parent, PFunction function)
        {
            if (function == null)
                throw new ArgumentNullException("function");
            if (parent == null)
                throw new ArgumentNullException("parent");
            Parent = parent;
            Function = function;
            var m = function.Meta;
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
            if (obj == null)
                return false;
            var be = obj as BenchmarkEntry;
            return be != null && Function.Equals(be.Function);
        }

        public override int GetHashCode()
        {
            return Function.GetHashCode() ^ 12;
        }

        public static bool operator ==(BenchmarkEntry be1, BenchmarkEntry be2)
        {
            if ((object) be1 == null && (object) be2 == null)
                return true;
            else if ((object) be1 == null || (object) be2 == null)
                return false;
            else
                return be1.Equals(be2);
        }

        public static bool operator !=(BenchmarkEntry be1, BenchmarkEntry be2)
        {
            if ((object) be1 == null && (object) be2 == null)
                return false;
            else if ((object) be1 == null || (object) be2 == null)
                return true;
            else
                return !(be1.Equals(be2));
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
                    "--------------------------------------\n{0}\n {1}", Title, Description);
                if (UsesIteration)
                    Console.WriteLine("\tIterations:\t{0}", iterations);
            }

            var argv = new PValue[] {iterations};
            sw.Reset();
            sw.Start();
            Function.Run(Parent.Machine, argv);
            sw.Stop();

            var raw = sw.ElapsedMilliseconds;
            long overhead = 0;
            if (Overhead != null && Parent.Entries.Contains(Overhead))
                overhead = Parent.Entries[Overhead].GetAverageRawMilliseconds();

            var m = new Measurement(this, raw, overhead);

            if (verbose)
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
                if (overhead > 0)
                    Console.WriteLine(
                        "\tcleared:\t{0:0.00} ms\n" +
                            "\t\t\t{1:0.00} micros",
                        m.ClearedPassMilliseconds,
                        m.ClearedPassMicroseconds);
            }

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);

            return m;
        }

        #endregion

        public void WarmUp()
        {
            var fctx =
                Function.CreateFunctionContext(
                    Parent.Machine, new PValue[] {Benchmark.DefaultWarmUpIterations});
            Parent.Machine.Process(fctx);
        }
    }
}