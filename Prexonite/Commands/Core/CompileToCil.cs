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
using System.Diagnostics.CodeAnalysis;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands.Core
{
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = "Cil")]
    public class CompileToCil : PCommand, ICilCompilerAware
    {
        #region Singleton

        private CompileToCil()
        {
        }

        public static CompileToCil Instance { get; } = new();

        #endregion

        [Obsolete]
        public override bool IsPure => false;

        public static bool AlreadyCompiledStatically { get; private set; }

        #region ICilCompilerAware Members

        public CompilationFlags CheckQualification(Instruction ins)
        {
            return CompilationFlags.PrefersRunStatically;
        }

        public void ImplementInCil(CompilerState state, Instruction ins)
        {
            throw new NotSupportedException();
        }

        #endregion

        /// <summary>
        ///     Executes the command.
        /// </summary>
        /// <param name = "sctx">The stack context in which to execute the command.</param>
        /// <param name = "args">The arguments to be passed to the command.</param>
        /// <returns>The value returned by the command. Must not be null. (But possibly {null~Null})</returns>
        public override PValue Run(StackContext sctx, PValue[] args)
        {
            return RunStatically(sctx, args);
        }

        /// <summary>
        ///     Executes the command.
        /// </summary>
        /// <param name = "sctx">The stack context in which to execute the command.</param>
        /// <param name = "args">The arguments to be passed to the command.</param>
        /// <returns>The value returned by the command. Must not be null. (But possibly {null~Null})</returns>
        /// <remarks>
        ///     <para>
        ///         This variation is independent of the executing engine and can take advantage from static binding in CIL compilation.
        ///     </para>
        /// </remarks>
        public static PValue RunStatically(StackContext sctx, PValue[] args)
        {
            if (sctx == null)
                throw new ArgumentNullException(nameof(sctx));
            args ??= Array.Empty<PValue>();

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
                                    $"You should only use static compilation once per process. Use {Engine.CompileToCilAlias}(true)" +
                                    " to force recompilation (warning: memory leak!). Should your program recompile dynamically, " +
                                    $"use {Engine.CompileToCilAlias}(false) for disposable implementations.");
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
                                if (
                                    !sctx.ParentApplication.Functions.TryGetValue(
                                        (string) arg.Value, out func))
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

                        Compiler.Cil.Compiler.TryCompile(func, sctx.ParentEngine,
                            FunctionLinking.FullyIsolated);
                    }
                    break;
            }

            return PType.Null;
        }
    }
}