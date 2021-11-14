// Prexonite
// 
// Copyright (c) 2014, Christian Klauser
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, 
//  are permitted provided that the following conditions are met:
// 
//     Redistributions of source code must retain the above copyright notice, 
//          this list of conditions and the following disclaimer.
//     Redistributions in binary form must reproduce the above copyright notice, 
//          this list of conditions and the following disclaimer in the 
//          documentation and/or other materials provided with the distribution.
//     The names of the contributors may be used to endorse or 
//          promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
//  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
//  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
//  IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
//  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
//  (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
//  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
//  WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING 
//  IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
using System.Reflection;
using System.Reflection.Emit;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands.Core.Operators;

public abstract class BinaryOperatorBase : PCommand, ICilExtension
{
    public bool ValidateArguments(CompileTimeValue[] staticArgv, int dynamicArgc)
    {
        return staticArgv.Length + dynamicArgc >= 2;
    }

    /// <summary>
    ///     The method that should be called on the left-hand-side value. Must have signature (StackContext, PValue).
    /// </summary>
    protected abstract MethodInfo OperationMethod { get; }

    public virtual void Implement(CompilerState state, Instruction ins,
        CompileTimeValue[] staticArgv, int dynamicArgc)
    {
        if (dynamicArgc >= 2)
        {
            state.EmitIgnoreArguments(dynamicArgc - 2);
            state.EmitStoreLocal(state.PrimaryTempLocal);
            state.EmitLoadLocal(state.SctxLocal);
            state.EmitLoadLocal(state.PrimaryTempLocal);
        }
        else if (dynamicArgc == 1)
        {
            //we can load the second static arg just where we need it
            state.EmitLoadLocal(state.SctxLocal);
            staticArgv[0].EmitLoadAsPValue(state);
        }
        else
        {
            if (staticArgv[0].TryGetConstant(out var left)
                && staticArgv[1].TryGetConstant(out var right))
            {
                //Both operands are constants (remember: static args can also be references)
                //=> Apply the operator at compile time.
                var result = Run(state, new[] {left, right});
                switch (result.Type.ToBuiltIn())
                {
                    case PType.BuiltIn.Real:
                        state.EmitLoadRealAsPValue((double) result.Value);
                        break;
                    case PType.BuiltIn.Int:
                        state.EmitLoadIntAsPValue((int) result.Value);
                        break;
                    case PType.BuiltIn.String:
                        state.EmitLoadStringAsPValue((string) result.Value);
                        break;
                    case PType.BuiltIn.Null:
                        state.EmitLoadNullAsPValue();
                        break;
                    case PType.BuiltIn.Bool:
                        state.EmitLoadBoolAsPValue((bool) result.Value);
                        break;
                    default:
                        throw new PrexoniteException(
                            $"The operation {GetType().FullName} is no implemented correctly. Given {left} and {right} it results in the non-constant {result}");
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