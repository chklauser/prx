using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.Lazy
{
    /// <summary>
    /// Turns values in WHNF into thunks and leaves existing thunks alone. This helps
    /// building functions that can be callled with both strict and lazy arguments.
    /// </summary>
    public class AsThunkCommand : PCommand, ICilCompilerAware
    {
        #region Singleton pattern

        private AsThunkCommand()
        {
        }

        private static readonly AsThunkCommand _instance = new AsThunkCommand();

        public static AsThunkCommand Instance
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
            if (sctx == null) throw new ArgumentNullException("sctx");
            if (args == null || args.Length == 0 || args[0] == null)
                throw new PrexoniteException("The asThunk command requires a value.");

            return ThunkCommand._EnforceThunk(args[0]);
        }

        #endregion

        #region Implementation of ICilCompilerAware

        public CompilationFlags CheckQualification(Instruction ins)
        {
            return CompilationFlags.PrefersRunStatically;
        }

        public void ImplementInCil(CompilerState state, Instruction ins)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}