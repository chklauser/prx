using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands.Math
{
    public class Sin : PCommand, ICilCompilerAware
    {
        #region Singleton

        private Sin()
        {
        }

        private static readonly Sin _instance = new Sin();

        public static Sin Instance
        {
            get { return _instance; }
        }

        #endregion 

        /// <summary>
        /// A flag indicating whether the command acts like a pure function.
        /// </summary>
        /// <remarks>Pure commands can be applied at compile time.</remarks>
        public override bool IsPure
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="sctx">The stack context in which to execut the command.</param>
        /// <param name="args">The arguments to be passed to the command.</param>
        /// <returns>The value returned by the command. Must not be null. (But possibly {null~Null})</returns>
        public static PValue RunStatically(StackContext sctx, PValue[] args)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (args == null)
                throw new ArgumentNullException("args");

            if (args.Length < 1)
                throw new PrexoniteException("Sin requires at least one argument.");

            PValue arg0 = args[0];

            return RunStatically(arg0, sctx);
        }

        public override PValue Run(StackContext sctx, PValue[] args)
        {
            return RunStatically(sctx, args);
        }

        public static PValue RunStatically(PValue arg0, StackContext sctx)
        {
            double x = (double)arg0.ConvertTo(sctx, PType.Real, true).Value;

            return System.Math.Sin(x);
        }

        #region ICilCompilerAware Members

        /// <summary>
        /// Asses qualification and preferences for a certain instruction.
        /// </summary>
        /// <param name="ins">The instruction that is about to be compiled.</param>
        /// <returns>A set of <see cref="CompilationFlags"/>.</returns>
        CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
        {
            switch (ins.Arguments)
            {
                case 1:
                    return CompilationFlags.PreferCustomImplementation;
                case 0:
                default:
                    return CompilationFlags.PreferRunStatically;
            }
        }

        private static readonly MethodInfo RunStaticallyMethod =
            typeof(Sin).GetMethod("RunStatically", new Type[] { typeof(PValue), typeof(StackContext) });

        /// <summary>
        /// Provides a custom compiler routine for emitting CIL byte code for a specific instruction.
        /// </summary>
        /// <param name="state">The compiler state.</param>
        /// <param name="ins">The instruction to compile.</param>
        void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
        {
            switch (ins.Arguments)
            {
                case 1:
                    break;
                case 0:
                default:
                    throw new NotSupportedException();
            }

            if (ins.JustEffect)
            {
                state.Il.Emit(OpCodes.Pop);
            }
            else
            {
                state.EmitLoadLocal(state.SctxLocal);
                state.EmitCall(RunStaticallyMethod);
            }
        }

        #endregion
    }
}
