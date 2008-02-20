using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.Core
{
    public class Meta : PCommand, ICilCompilerAware
    {
        #region Singleton pattern

        private Meta()
        {
        }

        private static readonly Meta _instance = new Meta();

        public static Meta Instance
        {
            get
            {
                return _instance;
            }
        }

        #endregion

        #region ICilCompilerAware Members

        CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
        {
            return CompilationFlags.HasCustomWorkaround;
        }

        void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
        {
            if (ins.Arguments > 0)
                throw new PrexoniteException("The meta command no longer accepts arguments."); 

            state.EmitLoadLocal(state.SctxLocal);
            state.EmitLoadArg(CompilerState.ParamSourceIndex);
            MethodInfo getMeta = typeof(PFunction).GetProperty("Meta").GetGetMethod();
            state.Il.EmitCall(OpCodes.Callvirt, getMeta, null);
            state.Il.EmitCall(OpCodes.Call, Compiler.Cil.Compiler.CreateNativePValue, null);
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
                return false;
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
            if(sctx == null)
                throw new ArgumentNullException("sctx");
            if(args != null && args.Length > 0)
                throw new PrexoniteException("The meta command no longer accepts arguments."); 

            FunctionContext fctx = sctx as FunctionContext;

            if(fctx == null)
                throw new PrexoniteException("The meta command uses dynamic features and can therefor only be called from a Prexonite function.");

            return fctx.CreateNativePValue(fctx.Implementation.Meta);
        }
    }
}
