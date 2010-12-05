using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands.Core
{
    public class Id : PCommand, ICilCompilerAware
    {
        #region Singleton pattern

        private static readonly Id _instance = new Id();

        public static Id Instance
        {
            get { return _instance; }
        }

        private Id()
        {
        }

        #endregion

        public const string Alias = "id";

        public static PValue RunStatically(StackContext sctx, PValue[] args)
        {
            return args.Length > 0 ? args[0] : PType.Null;
        }

        public override PValue Run(StackContext sctx, PValue[] args)
        {
            return RunStatically(sctx, args);
        }

        public CompilationFlags CheckQualification(Instruction ins)
        {
            return CompilationFlags.PrefersRunStatically;
        }

        public void ImplementInCil(CompilerState state, Instruction ins)
        {
            var argc = ins.Arguments;
            if(argc == 0)
                return;

            if (ins.JustEffect)
            {
                state.EmitIgnoreArguments(argc);
            }
            else
            {
                state.EmitIgnoreArguments(argc-1);
            }
        }
    }
}
