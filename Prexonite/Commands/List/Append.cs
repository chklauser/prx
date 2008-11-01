using System;
using System.Collections.Generic;
using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.List
{
    public class Append : CoroutineCommand , ICilCompilerAware
    {

        #region Singleton pattern

        private Append()
        {
        }

        private static readonly Append _instance = new Append();

        public static Append Instance
        {
            get { return _instance; }
        }

        #endregion

        public override bool IsPure
        {
            get { return false; }
        }

        protected override IEnumerable<PValue> CoroutineRun(StackContext sctx, PValue[] args)
        {
            return CoroutineRunStatically(sctx, args);
        }

        protected static IEnumerable<PValue> CoroutineRunStatically(StackContext sctx, PValue[] args)
        {
            if (args == null)
                yield break;
            if (sctx == null)
                throw new ArgumentNullException("sctx");

            foreach (var arg in args)
            {
                if(arg == null)
                    throw new ArgumentException("No element in args must be null.","args") ;
                var xs = Map._ToEnumerable(sctx, arg);
                if(xs == null)
                    continue;
                foreach (var x in xs)
                    yield return x;
            }
        }

        public CompilationFlags CheckQualification(Instruction ins)
        {
            return CompilationFlags.PreferRunStatically;
        }

        public void ImplementInCil(CompilerState state, Instruction ins)
        {
            throw new  NotSupportedException();
        }
    }
}
