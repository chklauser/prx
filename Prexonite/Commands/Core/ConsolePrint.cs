using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands.Core
{
    public class ConsolePrint : PCommand, ICilCompilerAware
    {
        #region Singleton

        private ConsolePrint()
        {
        }

        private static readonly ConsolePrint _instance = new ConsolePrint();

        public static ConsolePrint Instance
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
            get { return false; }
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="sctx">The stack context in which to execut the command.</param>
        /// <param name="args">The arguments to be passed to the command.</param>
        /// <returns>The value returned by the command. Must not be null. (But possibly {null~Null})</returns>
        public static PValue RunStatically(StackContext sctx, PValue[] args)
        {
            StringBuilder buffer = new StringBuilder();
            for (int i = 0; i < args.Length; i++)
            {
                PValue arg = args[i];
                buffer.Append(arg.Type is StringPType ? (string)arg.Value : arg.CallToString(sctx));
            }

            Console.Write(buffer);

            return buffer.ToString();
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
            switch(ins.Arguments)
            {
                case 0:
                case 1:
                    return CompilationFlags.PreferCustomImplementation;
                default:
                    return CompilationFlags.PreferRunStatically;
            }
        }

        /// <summary>
        /// Provides a custom compiler routine for emitting CIL byte code for a specific instruction.
        /// </summary>
        /// <param name="state">The compiler state.</param>
        /// <param name="ins">The instruction to compile.</param>
        void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
        {
            switch(ins.Arguments)
            {
                case 0:
                    if(!ins.JustEffect)
                    {
                        state.Il.Emit(OpCodes.Ldstr, "");
                        state.EmitWrapString();
                    }
                    break;
                case 1:
                    state.EmitLoadLocal(state.SctxLocal);
                    state.Il.EmitCall(OpCodes.Call, ConsolePrintLine.PValueCallToString, null);
                    if(!ins.JustEffect)
                    {
                        state.Il.Emit(OpCodes.Dup);
                        state.EmitWrapString();
                        state.EmitStoreTemp(0);
                    }
                    state.Il.EmitCall(OpCodes.Call, ConsolePrintLine.ConsoleWriteMethod, null);
                    if(!ins.JustEffect)
                    {
                        state.EmitLoadTemp(0);
                    }
                    break;
                default:
                    throw new NotSupportedException();
            }
                


        }

        #endregion
    }
}