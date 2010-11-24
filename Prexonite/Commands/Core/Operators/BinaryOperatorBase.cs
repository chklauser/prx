using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands.Core.Operators
{
    public abstract class BinaryOperatorBase : PCommand, ICilExtension
    {
        public override bool IsPure
        {
            get { return true; }
        }

        public bool ValidateArguments(CompileTimeValue[] staticArgv, int dynamicArgc)
        {
            return staticArgv.Length + dynamicArgc >= 2;
        }

        /// <summary>
        /// The method that should be called on the left-hand-side value. Must have signature (StackContext, PValue).
        /// </summary>
        protected abstract MethodInfo OperationMethod { get; }

        public virtual void Implement(CompilerState state, Instruction ins, CompileTimeValue[] staticArgv, int dynamicArgc)
        {
            if(dynamicArgc >= 2)
            {
                state.EmitIgnoreArguments(dynamicArgc-2);
                state.EmitStoreLocal(state.PrimaryTempLocal);
                state.EmitLoadLocal(state.SctxLocal);
                state.EmitLoadLocal(state.PrimaryTempLocal);
            }
            else if(dynamicArgc == 1)
            {
                //we can load the second static arg just where we need it
                state.EmitLoadLocal(state.SctxLocal);
                staticArgv[0].EmitLoadAsPValue(state);
            }
            else
            {
                PValue left;
                PValue right;

                if (staticArgv[0].TryGetConstant(out left)
                 && staticArgv[1].TryGetConstant(out right))
                {
                    //Both operands are constants (remember: static args can also be references)
                    //=> Apply the operator at compile time.
                    var result = Run(state, new[] { left, right });
                    switch (result.Type.ToBuiltIn())
                    {
                        case PType.BuiltIn.Real:
                            state.EmitLoadRealAsPValue((double)result.Value);
                            break;
                        case PType.BuiltIn.Int:
                            state.EmitLoadIntAsPValue((int)result.Value);
                            break;
                        case PType.BuiltIn.String:
                            state.EmitLoadStringAsPValue((string)result.Value);
                            break;
                        case PType.BuiltIn.Null:
                            state.EmitLoadNullAsPValue();
                            break;
                        case PType.BuiltIn.Bool:
                            state.EmitLoadBoolAsPValue((bool)result.Value);
                            break;
                        default:
                            throw new PrexoniteException(
                                string.Format(
                                    "The operation {0} is no implemented correctly. Given {1} and {2} it results in the non-constant {3}",
                                    GetType().FullName, left, right, result));
                    }
                    return; //We've already emitted the result. 
                }
                else
                {
                    //Load the first operand now, then proceed like for just one static arg
                    staticArgv[0].EmitLoadAsPValue(state);
                    state.EmitLoadLocal(state.SctxLocal);
                    staticArgv[1].EmitLoadAsPValue(state);
                }
            }

            state.Il.EmitCall(OpCodes.Call, OperationMethod, null);
        }
    }
}
