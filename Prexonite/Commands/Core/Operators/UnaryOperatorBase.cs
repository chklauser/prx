using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.Core.Operators
{
    public abstract class UnaryOperatorBase : PCommand, ICilExtension
    {
        public override bool IsPure
        {
            get { return true; }
        }

        public bool ValidateArguments(CompileTimeValue[] staticArgv, int dynamicArgc)
        {
            return staticArgv.Length + dynamicArgc >= 1;
        }

        /// <summary>
        /// The method that should be called on the value. Must have signature (StackContext).
        /// </summary>
        protected abstract MethodInfo OperationMethod { get; }

        public void Implement(CompilerState state, Instruction ins, CompileTimeValue[] staticArgv, int dynamicArgc)
        {
            if(dynamicArgc >= 1)
            {
                state.EmitIgnoreArguments(dynamicArgc-1);
            }
            else
            {
                staticArgv[0].EmitLoadAsPValue(state);
            }
            state.EmitLoadLocal(state.SctxLocal);
            state.Il.EmitCall(OpCodes.Call, OperationMethod, null);
        }
    }
}
