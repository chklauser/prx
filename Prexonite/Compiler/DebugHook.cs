// Prexonite
// 
// Copyright (c) 2011, Christian Klauser
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
// 
//     Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//     Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//     The names of the contributors may be used to endorse or promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using Prexonite.Commands.Core;
using Prexonite.Compiler.Ast;
using Prexonite.Types;

namespace Prexonite.Compiler
{
    /// <summary>
    ///     Implementation of a compiler hook that optimizes the using of the <see cref = "Debug" /> command.
    /// </summary>
    public static class DebugHook
    {
        /// <summary>
        ///     To determine whether a function allows debugging or not, this meta key is checked.
        /// </summary>
        public const string DebuggingMetaKey = "debugging";

        /// <summary>
        ///     Implementation of the debug hook.
        /// </summary>
        /// <param name = "t">The compiler target that is to be transformed.</param>
        public static void Hook(CompilerTarget t)
        {
            var debugging = IsDebuggingEnabled(t.Function);

            replace_debug(t, t.Ast, debugging);
        }

        private static readonly CompilerHook _hook = new CompilerHook(Hook);

        /// <summary>
        ///     Installs the hook in the supplied <see cref = "Loader" />.
        /// </summary>
        /// <param name = "ldr">The loader.</param>
        public static void InstallHook(Loader ldr)
        {
            if (ldr == null)
                throw new ArgumentNullException("ldr");
            ldr.CompilerHooks.Add(_hook);
        }

        /// <summary>
        ///     Uninstalls the hook in the supplied <see cref = "Loader" />.
        /// </summary>
        /// <param name = "ldr">The loader.</param>
        public static void UninstallHook(Loader ldr)
        {
            if (ldr == null)
                throw new ArgumentNullException("ldr");
            ldr.CompilerHooks.Remove(_hook);
        }

        /// <summary>
        ///     Determines whether a function allows debugging or not.
        /// </summary>
        /// <param name = "function">The function to be checked for debugging settings.</param>
        /// <returns>True if the function allows debugging, false otherwise.</returns>
        public static bool IsDebuggingEnabled(PFunction function)
        {
            if (function.Meta.ContainsKey(DebuggingMetaKey))
                return function.Meta[DebuggingMetaKey].Switch;
            else
                return function.ParentApplication.Meta[DebuggingMetaKey].Switch;
        }

        private static void replace_debug(CompilerTarget t, IList<AstNode> block, bool debugging)
        {
            for (var i = 0; i < block.Count; i++)
            {
                var stmt = block[i] as AstGetSetSymbol;
                //look for calls
                if (stmt != null && stmt.Interpretation == SymbolInterpretations.Command &&
                    Engine.StringsAreEqual(stmt.Id, Engine.DebugAlias))
                {
                    //Found a call to debug
                    block.RemoveAt(i);
                    if (debugging)
                    {
                        for (var j = 0; j < stmt.Arguments.Count; j++)
                        {
                            var arg = stmt.Arguments[j] as AstGetSetSymbol;
                            if (arg != null)
                            {
                                var printlnCall =
                                    new AstGetSetSymbol(
                                        stmt.File,
                                        stmt.Line,
                                        stmt.Column,
                                        PCall.Get,
                                        Engine.PrintLineAlias,
                                        SymbolInterpretations.Command);
                                var concatCall =
                                    new AstGetSetSymbol(
                                        stmt.File,
                                        stmt.Line,
                                        stmt.Column,
                                        PCall.Get,
                                        Engine.ConcatenateAlias,
                                        SymbolInterpretations.Command);

                                var consts =
                                    new AstConstant(
                                        stmt.File,
                                        stmt.Line,
                                        stmt.Column,
                                        String.Concat("DEBUG ", arg.Id, " = "));
                                concatCall.Arguments.Add(consts);
                                concatCall.Arguments.Add(arg);
                                printlnCall.Arguments.Add(concatCall);

                                block.Insert(i, printlnCall);
                                i += 1;
                            } //end if arg not null
                        } //end for arguments             
                    } //end if debugging

                    continue;
                } //end if debug call

                var cond = block[i] as AstCondition;

                //look for conditions
                if (cond != null)
                {
                    var expr = cond.Condition as AstGetSetSymbol;
                    if (expr != null && expr.Interpretation == SymbolInterpretations.Command &&
                        Engine.StringsAreEqual(expr.Id, Engine.DebugAlias))
                        cond.Condition =
                            new AstConstant(expr.File, expr.Line, expr.Column, debugging);
                }

                //Recursively replace 'debug' in nested blocks.
                var complex = block[i] as IAstHasBlocks;
                if (complex != null)
                    foreach (var subBlock in complex.Blocks)
                        replace_debug(t, subBlock, debugging);
            } //end for statements
        }
    }
}