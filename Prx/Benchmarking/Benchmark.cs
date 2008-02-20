using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;

using Prexonite;
using Prexonite.Types;

namespace Prx.Benchmarking
{
    public sealed class Benchmark : IObject
    {

        #region Construction

        private Engine _machine;

        public Engine Machine
        {
            get
            {
                return _machine;
            }
            set
            {
                _machine = value;
            }
        }
	

        private int _iterations;

        public int Iterations
        {
            get
            {
                return _iterations;
            }
        }

        public Benchmark(StackContext sctx, int iterations)
            : this(sctx.ParentEngine, iterations)
        {
        }

        public Benchmark(Engine eng, int iterations)
        {
            if (eng == null)
                throw new ArgumentNullException("eng"); 
            if (iterations < 0)
                throw new ArgumentOutOfRangeException(
                    "iterations", iterations, "iterations must be a positive integer.");
            _iterations = iterations;
            _machine = eng;
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
        public const int DefaultWarmUpIterations = 50;

        private readonly BenchmarkEntryCollection entries = new BenchmarkEntryCollection();
        public BenchmarkEntryCollection Entries
        {
            get
            {
                return entries;
            }
        }

        internal Stopwatch _Stopwatch
        {
            get { return __Stopwatch; }
        }

        private readonly Stopwatch __Stopwatch = new Stopwatch();

        public void IncludeAll(Application application)
        {
            if (application == null)
                throw new ArgumentNullException("application");
            foreach (PFunction function in application.Functions)
                if (function.Meta[BenchmarkKey])
                    Include(function);
        }

        public void IncludeRange(params PFunction[] functions)
        {
            if (functions == null)
                throw new ArgumentNullException("functions");
            foreach (PFunction function in functions)
                Include(function);
        }

        public  void Include(PFunction function)
        {
            if (function == null)
                throw new ArgumentNullException("function"); 
            entries.Add(new BenchmarkEntry(this,function));
        }

        public List<Measurement> MeasureAll(bool verbose)
        {
            List<Measurement> lst = new List<Measurement>(Entries.Count);
            foreach (BenchmarkEntry entry in entries)
                lst.Add(entry.Measure(verbose));
            return lst;
        }

        public void WarmUp()
        {
            foreach (BenchmarkEntry entry in entries)
                entry.WarmUp();
        }

        #endregion

        #region IObject Members

        public bool TryDynamicCall(
            StackContext sctx, PValue[] args, PCall call, string id, out PValue result)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (args == null)
                args = new PValue[0];
            if (id == null)
                id = "";

            result = null;
            switch(id.ToLower(CultureInfo.InvariantCulture))
            {
                case "include":
                    foreach (PValue arg in args)
                        Include(arg.ConvertTo<PFunction>(sctx));
                    result = PType.Null.CreatePValue();
                    break;
                case "includerange":
                    foreach (PValue arg in args)
                    {
                        if(arg.Type.ToBuiltIn() != PType.BuiltIn.List)
                            continue;
                        foreach (PValue func in (List<PValue>)arg.Value)
                            Include(func.ConvertTo<PFunction>(sctx));
                    }
                    result = PType.Null.CreatePValue();
                    break;
                case "includeall":
                    Application tapp;
                    if(!(args.Length > 0 && args[0].TryConvertTo(sctx, out tapp)))
                        tapp = sctx.ParentApplication;
                    IncludeAll(tapp);
                    result = PType.Null.CreatePValue();
                    break;
                case "measureall":
                    bool verbose;
                    if(!(args.Length > 0 && args[0].TryConvertTo(sctx, out verbose)))
                        verbose = true;
                    List<PValue> plst = new List<PValue>();
                    foreach (Measurement m in MeasureAll(verbose))
                        plst.Add(sctx.CreateNativePValue(m));
                    result = (PValue)plst;
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
