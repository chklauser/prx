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
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands.Core
{
    public class CompileToCil : PCommand, ICilCompilerAware
    {
        #region Singleton

        private static readonly CompileToCil _instance = new CompileToCil();

        private CompileToCil()
        {
        }

        public static CompileToCil Instance
        {
            get { return _instance; }
        }

        #endregion

        public override bool IsPure
        {
            get { return false; }
        }

        public static bool AlreadyCompiledStatically { get; private set; }

        #region ICilCompilerAware Members

        public CompilationFlags CheckQualification(Instruction ins)
        {
            return CompilationFlags.PreferRunStatically;
        }

        public void ImplementInCil(CompilerState state, Instruction ins)
        {
            throw new NotSupportedException();
        }

        #endregion

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

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="sctx">The stack context in which to execut the command.</param>
        /// <param name="args">The arguments to be passed to the command.</param>
        /// <returns>The value returned by the command. Must not be null. (But possibly {null~Null})</returns>
        /// <remarks>
        ///     <para>
        ///         This variation is independant of the executing engine and can take advantage from static binding in CIL compilation.
        ///     </para>
        /// </remarks>
        public static PValue RunStatically(StackContext sctx, PValue[] args)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (args == null)
                args = new PValue[] {};

            var linking = FunctionLinking.FullyStatic;
            switch (args.Length)
            {
                case 0:
                    //come from case 1
                    if (sctx.ParentEngine.StaticLinkingAllowed)
                    {
                        if (args.Length == 0)
                        {
                            if (AlreadyCompiledStatically)
                                throw new PrexoniteException
                                    (
                                    string.Format
                                        (
                                        "You should only use static compilation once per process. Use {0}(true)" +
                                        " to force recompilation (warning: memory leak!). Should your program recompile dynamically, " +
                                        "use {1}(false) for disposable implementations.",
                                        Engine.CompileToCilAlias,
                                        Engine.CompileToCilAlias));
                            else
                                AlreadyCompiledStatically = true;
                        }
                    }
                    else
                    {
                        linking = FunctionLinking.FullyIsolated;
                    }
                    Compiler.Cil.Compiler.Compile(sctx.ParentApplication, sctx.ParentEngine, linking);
                    break;
                case 1:
                    var arg0 = args[0];

                    if (arg0 == null || arg0.IsNull)
                        goto case 0;
                    if (arg0.Type == PType.Bool)
                    {
                        if ((bool) arg0.Value)
                            linking = FunctionLinking.FullyStatic;
                        else
                            linking = FunctionLinking.FullyIsolated;
                        goto case 0;
                    }
                    else if (arg0.Type == typeof (FunctionLinking))
                    {
                        linking = (FunctionLinking) arg0.Value;
                        goto case 0;
                    }
                    else
                    {
                        goto default;
                    }
                default:
                    //Compile individual functions to CIL
                    foreach (var arg in args)
                    {
                        var T = arg.Type;
                        PFunction func;
                        switch (T.ToBuiltIn())
                        {
                            case PType.BuiltIn.String:
                                if (!sctx.ParentApplication.Functions.TryGetValue((string) arg.Value, out func))
                                    continue;
                                break;
                            case PType.BuiltIn.Object:
                                func = arg.Value as PFunction;
                                if (func == null)
                                    goto default;
                                else
                                    break;
                            default:
                                if (!arg.TryConvertTo(sctx, out func))
                                    continue;
                                break;
                        }

                        Compiler.Cil.Compiler.TryCompile(func, sctx.ParentEngine, FunctionLinking.FullyIsolated);
                    }
                    break;
            }

            return PType.Null;
        }
    }
}