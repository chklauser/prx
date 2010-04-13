using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands.Lazy
{
    public class ForceCommand : PCommand, ICilCompilerAware
    {
        #region Singleton pattern

        private ForceCommand()
        {
        }

        private static readonly ForceCommand _instance = new ForceCommand();

        public static ForceCommand Instance
        {
            get { return _instance; }
        }

        #endregion

        #region Overrides of PCommand

        public override bool IsPure
        {
            get { return false; }
        }

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
            if (args.Length < 1)
                throw new PrexoniteException("force requires an argument.");

            var arg = args[0] ?? PType.Null;
            if (arg.IsNull)
                return PType.Null;

            return Force(sctx, arg);
        }

        public static PValue Force(StackContext sctx, PValue arg)
        {
            var t = arg.Value as Thunk;

            var result = t != null ? t.Force(sctx) : arg;

            Debug.Assert(!(result.Value is Thunk), "Force wanted to return an unevaluated thunk.");

            return result;
        }

        #endregion

        #region Implementation of ICilCompilerAware

        CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
        {
            return CompilationFlags.PreferRunStatically;
        }

        void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}