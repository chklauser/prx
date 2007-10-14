using System;
using Prexonite.Commands;
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

        private static CompilerHook _hook = new CompilerHook(Hook);

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

        private static void replace_debug(CompilerTarget t, AstBlock block, bool debugging)
        {
            for (int i = 0; i < block.Count; i++)
            {
                AstGetSetSymbol stmt = block[i] as AstGetSetSymbol;
                //look for calls
                if (stmt != null && stmt.Interpretation == SymbolInterpretations.Command &&
                    Engine.StringsAreEqual(stmt.Id, Engine.DebugCommand))
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
                                        Engine.PrintLineCommand,
                                        SymbolInterpretations.Command);
                                AstGetSetSymbol concatCall =
                                    new AstGetSetSymbol(
                                        stmt.File,
                                        stmt.Line,
                                        stmt.Column,
                                        PCall.Get,
                                        Engine.ConcatenateCommand,
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
                        Engine.StringsAreEqual(expr.Id, Engine.DebugCommand))
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