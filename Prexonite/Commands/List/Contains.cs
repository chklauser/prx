using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands.List
{
    public class Contains : PCommand, ICilCompilerAware
    {
        public const string Alias = "contains";

        #region Singleton pattern

        private static readonly Contains _instance = new Contains();

        public static Contains Instance
        {
            get { return _instance; }
        }

        private Contains()
        {
        }

        #endregion

        public override PValue Run(StackContext sctx, PValue[] args)
        {
            return RunStatically(sctx, args);
        }

        public static PValue RunStatically(StackContext sctx, PValue[] args)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (args == null)
                throw new ArgumentNullException("args");

            PValue needle;
            if (args.Length < 2) 
                return false;
            else 
                needle = args[0];

            foreach (var arg in args.Skip(1))
            {
                var set = Map._ToEnumerable(sctx, arg);
                if(set!=null)
                    foreach (var value in set)
                    {
                        PValue result;
                        bool boolResult;
                        if (value.Equality(sctx, needle, out result) &&
                            result.TryConvertTo(sctx, true, out boolResult) && boolResult)
                            return result;
                    }
            }

            return false;
        }

        public CompilationFlags CheckQualification(Instruction ins)
        {
            return CompilationFlags.PrefersCustomImplementation;
        }

        public void ImplementInCil(CompilerState state, Instruction ins)
        {
            throw new NotImplementedException();
        }
    }
}
