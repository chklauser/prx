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
using System;
using System.Reflection.Emit;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands.Core.PartialApplication
{
    /// <summary>
    ///     <para>Common base class for partial application commands (constructors) that deal with an additional PType parameter (such as type casts)</para>
    ///     <para>This class exists to share implementation. DO NOT use it for classification.</para>
    /// </summary>
    /// <typeparam name = "T">The parameter POCO. <see cref = "PTypeInfo" /> can be used if no additional information is required.</typeparam>
    public abstract class PartialWithPTypeCommandBase<T> : PartialApplicationCommandBase<T>
        where T : PTypeInfo, new()
    {
        /// <summary>
        ///     The human readable name of this kind of partial application. Used in error messages.
        /// </summary>
        protected abstract string PartialApplicationKind { get; }

        protected override T FilterRuntimeArguments(StackContext sctx,
            ref ArraySegment<PValue> arguments)
        {
            if (arguments.Count < 1)
            {
                throw new PrexoniteException(
                    $"{PartialApplicationKind} requires a PType argument (or a PType expression).");
            }

            var raw = arguments.Array[arguments.Offset + arguments.Count - 1];
            PType ptype;
            //Allow the type to be specified as a type expression (instead of a type instance)
            if (!(raw.Type is ObjectPType && (object) (ptype = raw.Value as PType) != null))
            {
                var ptypeExpr = raw.CallToString(sctx);
                ptype = sctx.ConstructPType(ptypeExpr);
            }

            arguments = new ArraySegment<PValue>(arguments.Array, arguments.Offset,
                arguments.Count - 1);
            return new T {Type = ptype};
        }

        protected override bool FilterCompileTimeArguments(
            ref ArraySegment<CompileTimeValue> staticArgv, out T parameter)
        {
            parameter = default;
            if (staticArgv.Count < 1)
                return false;

            var raw = staticArgv.Array[staticArgv.Offset + staticArgv.Count - 1];
            if (!raw.TryGetString(out var ptypeExpr))
                return false;

            parameter = new T {Expr = ptypeExpr};
            staticArgv = new ArraySegment<CompileTimeValue>(staticArgv.Array, staticArgv.Offset,
                staticArgv.Count - 1);
            return true;
        }

        protected override void EmitConstructorCall(CompilerState state, T parameter)
        {
            state.EmitLoadLocal(state.SctxLocal);
            state.Il.Emit(OpCodes.Ldstr, parameter.Expr);
            state.EmitCall(Runtime.ConstructPTypeMethod);
            base.EmitConstructorCall(state, parameter);
        }
    }
}