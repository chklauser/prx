// Prexonite
// 
// Copyright (c) 2011, Christian Klauser
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
using System.Linq;
using System.Reflection.Emit;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands.Core
{
    /// <summary>
    ///     Unbinds a variable from closures using it.
    /// </summary>
    /// <example>
    ///     <code>function main()
    ///         {
    ///         var n = 15;
    ///         function f1()
    ///         {
    ///         while(n > 4)
    ///         println(n--);
    ///         }
    ///     
    ///         f1();
    ///         println(n); //"4"
    /// 
    ///         n = 13;
    ///         unbind(->n);
    ///         f1();
    ///         println(n); //"13"
    ///         }</code>After the call to unbind, $n does no longer 
    ///     refer to the same variable as the closure but 
    ///     still represents the same value.
    /// </example>
    /// <remarks>
    ///     <para>What unbind does, is to copy the contents of the 
    ///         supplied variable to a new memory location and associate 
    ///         all references <b>inside the calling function</b> with this 
    ///         new memory location. Should a function create two closures before 
    ///         calling unbind on a shared variable, those two closures will still 
    ///         use the same memory location. Only the references in the calling 
    ///         function change.</para>
    ///     <para>Important: that the value of the variable remains untouched. 
    ///         The <see cref = "PValue" /> object reference is just copied to 
    ///         the new memory location.</para>
    /// </remarks>
    public sealed class Unbind : PCommand, ICilCompilerAware, ICilExtension
    {
        private Unbind()
        {
        }

        private static readonly Unbind _instance = new Unbind();

        public static Unbind Instance
        {
            get { return _instance; }
        }

        /// <summary>
        ///     Executes the unbind command on each of the arguments supplied.
        /// </summary>
        /// <param name = "sctx">The <see cref = "FunctionContext" /> to modify.</param>
        /// <param name = "args">A list of local variable names or references.</param>
        /// <returns>Always {~Null}.</returns>
        /// <remarks>
        ///     Each of the supplied arguments is processed individually.
        /// </remarks>
        /// <exception cref = "ArgumentNullException">args is null</exception>
        public override PValue Run(StackContext sctx, PValue[] args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            foreach (var arg in args)
                Run(sctx, arg);
            return PType.Null.CreatePValue();
        }

        /// <summary>
        ///     Executes the unbind command on a <see cref = "PValue" /> argument.
        ///     The argument must either be the variable's name as a string or a 
        ///     reference to the <see cref = "PVariable" /> object.
        /// </summary>
        /// <param name = "sctx">The <see cref = "FunctionContext" /> to modify.</param>
        /// <param name = "arg">A variable reference or name.</param>
        /// <returns>Always {~Null}</returns>
        /// <exception cref = "ArgumentNullException"><paramref name = "sctx" /> is null</exception>
        /// <exception cref = "ArgumentNullException"><paramref name = "arg" /> is null</exception>
        /// <exception cref = "PrexoniteException"><paramref name = "arg" /> contains null</exception>
        /// <exception cref = "PrexoniteException"><paramref name = "sctx" /> is not a <see cref = "FunctionContext" /></exception>
        public static PValue Run(StackContext sctx, PValue arg)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");

            if (arg == null)
                throw new ArgumentNullException("arg");

            if (arg.IsNull)
                throw new PrexoniteException("The unbind command cannot process Null.");

            var fctx = sctx as FunctionContext;
            if (fctx == null)
                throw new PrexoniteException(
                    "The unbind command can only work on function contexts.");

            string id;

            if (arg.Type is ObjectPType && arg.Value is PVariable)
            {
                //Variable reference
                id = (from pair in fctx.LocalVariables
                      where ReferenceEquals(pair.Value, arg.Value)
                      select pair.Key
                    ).FirstOrDefault();
            }
            else
            {
                throw new PrexoniteException("Unbind requires variable references as arguments.");
            }

            PVariable existing;
            if (id != null && fctx.LocalVariables.TryGetValue(id, out existing))
            {
                var unbound = new PVariable {Value = existing.Value};
                fctx.ReplaceLocalVariable(id, unbound);
            }

            return PType.Null.CreatePValue();
        }

        #region ICilCompilerAware Members

        public CompilationFlags CheckQualification(Instruction ins)
        {
            return CompilationFlags.IsIncompatible;
        }

        public void ImplementInCil(CompilerState state, Instruction ins)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region Implementation of ICilExtension

        bool ICilExtension.ValidateArguments(CompileTimeValue[] staticArgv, int dynamicArgc)
        {
            return dynamicArgc == 0
                &&
                staticArgv.All(
                    arg => arg.Interpretation == CompileTimeInterpretation.LocalVariableReference);
        }

        void ICilExtension.Implement(CompilerState state, Instruction ins,
            CompileTimeValue[] staticArgv, int dynamicArgc)
        {
            foreach (var compileTimeValue in staticArgv)
            {
                string localVariableId;
                if (!compileTimeValue.TryGetLocalVariableReference(out localVariableId))
                    throw new ArgumentException(
                        "CIL implementation of Core.Unbind command only accepts local variable references.",
                        "staticArgv");

                Symbol symbol;
                if (!state.Symbols.TryGetValue(localVariableId, out symbol) ||
                    symbol.Kind != SymbolKind.LocalRef)
                    throw new PrexoniteException(
                        "CIL implementation of Core.Unbind cannot find local explicit variable " +
                            localVariableId);

                //Create new PVariable
                state.Il.Emit(OpCodes.Newobj, Compiler.Cil.Compiler.NewPVariableCtor);
                state.Il.Emit(OpCodes.Dup);

                //Copy old value
                state.EmitLoadPValue(symbol);
                state.EmitCall(Compiler.Cil.Compiler.SetValueMethod);

                //Override variable slot
                state.EmitStoreLocal(symbol.Local);
            }

            if (!ins.JustEffect)
                state.EmitLoadNullAsPValue();
        }

        #endregion
    }
}