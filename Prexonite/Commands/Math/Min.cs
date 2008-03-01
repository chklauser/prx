using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands.Math
{
    public class Min : PCommand, ICilCompilerAware
    {
        #region Singleton

        private Min()
        {
        }

        private static readonly Min _instance = new Min();

        public static Min Instance
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

            if (args.Length < 2)
                throw new PrexoniteException("Min requires at least two arguments.");

            PValue arg0 = args[0];
            PValue arg1 = args[1];
            return RunStatically(arg0, arg1, sctx);
        }

        public static PValue RunStatically(PValue arg0, PValue arg1, StackContext sctx)
        {
            if (arg0.Type == PType.Int && arg1.Type == PType.Int)
            {
                int a = (int)arg0.Value;
                int b = (int)arg1.Value;

                return System.Math.Min(a, b);
            }
            else
            {
                double a = (double)arg0.ConvertTo(sctx, PType.Real, true).Value;
                double b = (double)arg1.ConvertTo(sctx, PType.Real, true).Value;

                return System.Math.Min(a, b);
            }
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="sctx">The stack context in which to execut the command.</param>
        /// <param name="args">The arguments to be passed to the command.</param>
        /// <returns>The value returned by the command. Must not be null. (But possibly {null~Null})</returns>
        public override PValue Run(StackContext sctx, PValue[] args)
        {
            return RunStatically(sctx, args);
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
                case 0:
                case 1:
                case 2:
                    return CompilationFlags.PreferCustomImplementation;
                default:
                    return CompilationFlags.PreferRunStatically;
            }
        }

        private static readonly MethodInfo RunStaticallyMethod =
            typeof(Min).GetMethod("RunStatically", new Type[] { typeof(PValue), typeof(PValue), typeof(StackContext) });

        /// <summary>
        /// Provides a custom compiler routine for emitting CIL byte code for a specific instruction.
        /// </summary>
        /// <param name="state">The compiler state.</param>
        /// <param name="ins">The instruction to compile.</param>
        void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
        {
            if (ins.JustEffect)
            {
                for (int i = 0; i < ins.Arguments; i++)
                    state.Il.Emit(OpCodes.Pop);
            }
            else
            {
                switch (ins.Arguments)
                {
                    case 0:
                        state.EmitLoadPValueNull();
                        state.EmitLoadPValueNull();
                        break;
                    case 1:
                        state.EmitLoadPValueNull();
                        break;
                    case 2:
                        break;
                    default:
                        throw new NotSupportedException();
                }

                state.EmitLoadLocal(state.SctxLocal);
                state.EmitCall(RunStaticallyMethod);
            }
        }

        #endregion
    }
}
