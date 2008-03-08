using System;
using System.Collections.Generic;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands.List
{
    public class Skip : CoroutineCommand, ICilCompilerAware
    {
        #region Singleton

        private Skip()
        {
        }

        private static readonly Skip _instance = new Skip();

        public static Skip Instance
        {
            get { return _instance; }
        }

        #endregion 

        protected override IEnumerable<PValue> CoroutineRun(StackContext sctx, PValue[] args)
        {
            return CoroutineRunStatically(sctx, args);
        }

        protected static IEnumerable<PValue> CoroutineRunStatically(StackContext sctx, PValue[] args)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (args == null)
                throw new ArgumentNullException("args");

            int i = 0;
            if (args.Length < 1)
                throw new PrexoniteException("Skip requires at least one argument.");

            int index = (int)args[0].ConvertTo(sctx, PType.Int, true).Value;

            for (int j = 1; j < args.Length; j++)
            {
                PValue arg = args[j];
                IEnumerable<PValue> set = Map._ToEnumerable(sctx, arg);
                if (set == null)
                    throw new PrexoniteException(arg + " is neither a list nor a coroutine.");
                foreach (PValue value in set)
                {
                    if (i++ >= index)
                        yield return value;
                }
            }
        }

        public static PValue RunStatically(StackContext sctx, PValue[] args)
        {
            CoroutineContext corctx = new CoroutineContext(sctx, CoroutineRunStatically(sctx, args));
            return sctx.CreateNativePValue(new Coroutine(corctx));
        }

        /// <summary>
        /// A flag indicating whether the command acts like a pure function.
        /// </summary>
        /// <remarks>Pure commands can be applied at compile time.</remarks>
        public override bool IsPure
        {
            get { return false; }
        }

        #region ICilCompilerAware Members

        /// <summary>
        /// Asses qualification and preferences for a certain instruction.
        /// </summary>
        /// <param name="ins">The instruction that is about to be compiled.</param>
        /// <returns>A set of <see cref="CompilationFlags"/>.</returns>
        CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
        {
            return CompilationFlags.PreferRunStatically;
        }

        /// <summary>
        /// Provides a custom compiler routine for emitting CIL byte code for a specific instruction.
        /// </summary>
        /// <param name="state">The compiler state.</param>
        /// <param name="ins">The instruction to compile.</param>
        void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
