/*
 * Prexonite, a scripting engine (Scripting Language -> Bytecode -> Virtual Machine)
 *  Copyright (C) 2007  Christian "SealedSun" Klauser
 *  E-mail  sealedsun a.t gmail d.ot com
 *  Web     http://www.sealedsun.ch/
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  Please contact me (sealedsun a.t gmail do.t com) if you need a different license.
 * 
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

using System;
using System.Collections.Generic;
using Prexonite.Commands.Core;
using Prexonite.Compiler.Ast;
using Prexonite.Types;

namespace Prexonite.Compiler
{
    /// <summary>
    /// Implementation of a compiler hook that optimizes the using of the <see cref="Debug"/> command.
    /// </summary>
    public static class DebugHook
    {
        /// <summary>
        /// To determine whether a function allows debugging or not, this meta key is checked.
        /// </summary>
        public const string DebuggingMetaKey = "debugging";

        /// <summary>
        /// Implementation of the debug hook.
        /// </summary>
        /// <param name="t">The compiler target that is to be transformed.</param>
        public static void Hook(CompilerTarget t)
        {
            bool debugging = IsDebuggingEnabled(t.Function);

            replace_debug(t, t.Ast, debugging);
        }

        private static readonly CompilerHook _hook = new CompilerHook(Hook);

        /// <summary>
        /// Installs the hook in the supplied <see cref="Loader"/>.
        /// </summary>
        /// <param name="ldr">The loader.</param>
        public static void InstallHook(Loader ldr)
        {
            if (ldr == null)
                throw new ArgumentNullException("ldr");
            ldr.CompilerHooks.Add(_hook);
        }

        /// <summary>
        /// Uninstalls the hook in the supplied <see cref="Loader"/>.
        /// </summary>
        /// <param name="ldr">The loader.</param>
        public static void UninstallHook(Loader ldr)
        {
            if (ldr == null)
                throw new ArgumentNullException("ldr");
            ldr.CompilerHooks.Remove(_hook);
        }

        /// <summary>
        /// Determines whether a function allows debugging or not.
        /// </summary>
        /// <param name="function">The function to be checked for debugging settings.</param>
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
            for (int i = 0; i < block.Count; i++)
            {
                AstGetSetSymbol stmt = block[i] as AstGetSetSymbol;
                //look for calls
                if (stmt != null && stmt.Interpretation == SymbolInterpretations.Command &&
                    Engine.StringsAreEqual(stmt.Id, Engine.DebugAlias))
                {
                    //Found a call to debug
                    block.RemoveAt(i);
                    if (debugging)
                    {
                        for (int j = 0; j < stmt.Arguments.Count; j++)
                        {
                            AstGetSetSymbol arg = stmt.Arguments[j] as AstGetSetSymbol;
                            if (arg != null)
                            {
                                AstGetSetSymbol printlnCall =
                                    new AstGetSetSymbol(
                                        stmt.File,
                                        stmt.Line,
                                        stmt.Column,
                                        PCall.Get,
                                        Engine.PrintLineAlias,
                                        SymbolInterpretations.Command);
                                AstGetSetSymbol concatCall =
                                    new AstGetSetSymbol(
                                        stmt.File,
                                        stmt.Line,
                                        stmt.Column,
                                        PCall.Get,
                                        Engine.ConcatenateAlias,
                                        SymbolInterpretations.Command);

                                AstConstant consts =
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

                AstCondition cond = block[i] as AstCondition;

                //look for conditions
                if (cond != null)
                {
                    AstGetSetSymbol expr = cond.Condition as AstGetSetSymbol;
                    if (expr != null && expr.Interpretation == SymbolInterpretations.Command &&
                        Engine.StringsAreEqual(expr.Id, Engine.DebugAlias))
                        cond.Condition =
                            new AstConstant(expr.File, expr.Line, expr.Column, debugging);
                }

                //Recursively replace 'debug' in nested blocks.
                IAstHasBlocks complex = block[i] as IAstHasBlocks;
                if (complex != null)
                    foreach (AstBlock subBlock in complex.Blocks)
                        replace_debug(t, subBlock, debugging);
            } //end for statements
        }
    }
}