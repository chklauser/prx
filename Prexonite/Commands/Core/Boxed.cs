using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands.Core
{
    public class Boxed : PCommand, ICilCompilerAware
    {
        #region Singleton pattern

        private Boxed()
        {
        }

        private static readonly Boxed _instance = new Boxed();

        public static Boxed Instance
        {
            get { return _instance; }
        }

        #endregion 

        #region Overrides of PCommand

        [Obsolete]
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
            if (args.Length == 0)
                return PType.Null;

            var arg = args[0];
            if (arg == null)
                return PType.Null;

            return sctx.CreateNativePValue(arg);
        }

        #endregion

        #region Implementation of ICilCompilerAware

        CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
        {
            return CompilationFlags.PrefersRunStatically;
        }

        void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
        {
            throw new NotSupportedException("The command " + GetType().Name + " does not support CIL compilation via ICilCompilerAware.");
        }

        #endregion
    }
}
