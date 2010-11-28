using System;
using System.Collections.Generic;
using System.Linq;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands.List
{
    public class Sum : PCommand, ICilCompilerAware
    {
        #region Singleton pattern

        private Sum()
        {
        }

        private static readonly Sum _instance = new Sum();

        public static Sum Instance
        {
            get { return _instance; }
        }

        #endregion

        public virtual bool IsPure
        {
            get { return false; }
        }

        public override PValue Run(StackContext sctx, PValue[] args)
        {
            return RunStatically(sctx, args);
        }

        public static PValue RunStatically(StackContext sctx, PValue[] args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            if (sctx == null)
                throw new ArgumentNullException("sctx");

            //let sum xs acc = Seq.foldl (fun a b -> a + b) acc xs

            PValue acc;
            IEnumerable<PValue> xsArgs;

            if (args.Length == 0)
                return PType.Null;

            if (args.Length == 1)
            {
                acc = PType.Null;
                xsArgs = args;
            }
            else
            {
                acc = args[args.Length - 1];
                xsArgs = args.Take(args.Length - 1);
            }

            var xss = xsArgs.Select(e => Map._ToEnumerable(sctx, e)).Where(e => e != null);

            foreach (var xs in xss)
                foreach (var x in xs)
                    acc = acc.Addition(sctx, x);

            return acc;
        }

        public CompilationFlags CheckQualification(Instruction ins)
        {
            return CompilationFlags.PrefersRunStatically;
        }

        public void ImplementInCil(CompilerState state, Instruction ins)
        {
            throw new NotSupportedException();
        }
    }
}