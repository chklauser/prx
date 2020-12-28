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
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Prexonite;
using Prexonite.Types;

namespace Prx.Benchmarking
{
    public sealed class Benchmark : IObject
    {
        #region Construction

        public Engine Machine { get; set; }


        public int Iterations { get; }

        public Benchmark(StackContext sctx, int iterations)
            : this(sctx.ParentEngine, iterations)
        {
        }

        public Benchmark(Engine eng, int iterations)
        {
            if (iterations < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(iterations), iterations, "iterations must be a positive integer.");
            Iterations = iterations;
            Machine = eng ?? throw new ArgumentNullException(nameof(eng));
        }

        public Benchmark(Engine eng)
            : this(eng, DefaultIterations)
        {
        }

        public Benchmark(StackContext sctx)
            : this(sctx.ParentEngine)
        {
        }

        #endregion

        #region Entries

        public const string BenchmarkKey = "Benchmark";
        public const string TitleKey = "Title";
        public const string DescriptionKey = "Description";
        public const string UsesIterationKey = "Iteration";
        public const string OverheadKey = "Overhead";
        public const int DefaultIterations = 1000;
        public const int DefaultWarmUpIterations = 2;

        public BenchmarkEntryCollection Entries { get; } = new();

        internal Stopwatch _Stopwatch { get; } = new();

        public void IncludeAll(Application application)
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));
            foreach (var function in application.Functions)
                if (function.Meta[BenchmarkKey])
                    Include(function);
        }

        public void IncludeRange(params PFunction[] functions)
        {
            if (functions == null)
                throw new ArgumentNullException(nameof(functions));
            foreach (var function in functions)
                Include(function);
        }

        public void Include(PFunction function)
        {
            if (function == null)
                throw new ArgumentNullException(nameof(function));
            Entries.Add(new BenchmarkEntry(this, function));
        }

        public List<Measurement> MeasureAll(bool verbose)
        {
            var lst = new List<Measurement>(Entries.Count);
            foreach (var entry in Entries)
                lst.Add(entry.Measure(verbose));
            return lst;
        }

        public void WarmUp()
        {
            foreach (var entry in Entries)
                entry.WarmUp();
        }

        #endregion

        #region IObject Members

        public bool TryDynamicCall(
            StackContext sctx, PValue[] args, PCall call, string id, out PValue result)
        {
            if (sctx == null)
                throw new ArgumentNullException(nameof(sctx));
            args ??= Array.Empty<PValue>();
            id ??= "";

            result = null;
            switch (id.ToLower(CultureInfo.InvariantCulture))
            {
                case "include":
                    foreach (var arg in args)
                        Include(arg.ConvertTo<PFunction>(sctx));
                    result = PType.Null.CreatePValue();
                    break;
                case "includerange":
                    foreach (var arg in args)
                    {
                        if (arg.Type.ToBuiltIn() != PType.BuiltIn.List)
                            continue;
                        foreach (var func in (List<PValue>) arg.Value)
                            Include(func.ConvertTo<PFunction>(sctx));
                    }
                    result = PType.Null.CreatePValue();
                    break;
                case "includeall":
                    if (!(args.Length > 0 && args[0].TryConvertTo(sctx, out Application tapp)))
                        tapp = sctx.ParentApplication;
                    IncludeAll(tapp);
                    result = PType.Null.CreatePValue();
                    break;
                case "measureall":
                    if (!(args.Length > 0 && args[0].TryConvertTo(sctx, out bool verbose)))
                        verbose = true;
                    var plst = MeasureAll(verbose).Select(sctx.CreateNativePValue).ToList();
                    result = (PValue) plst;
                    break;
                case "warmup":
                    WarmUp();
                    result = PType.Null.CreatePValue();
                    break;
            }
            return result != null;
        }

        #endregion
    }
}