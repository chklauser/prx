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
using System.Collections.Generic;
using System.Diagnostics;
using Prexonite.Compiler;
using Prexonite.Compiler.Macro;
using Prexonite.Compiler.Macro.Commands;
using Prexonite.Types;

namespace Prexonite.Commands.Core
{
    public sealed class Call_Tail : StackAwareCommand
    {
        #region Singleton

        private Call_Tail()
        {
        }

        private static Call_Tail _instance = new Call_Tail();

        public static Call_Tail Instance
        {
            get { return _instance; }
        }

        #endregion

        public const string Alias = @"call\tail\perform";


        /// <summary>
        ///     Executes the command.
        /// </summary>
        /// <param name = "sctx">The stack context in which to execut the command.</param>
        /// <param name = "args">The arguments to be passed to the command.</param>
        /// <returns>The value returned by the command. Must not be null. (But possibly {null~Null})</returns>
        public override PValue Run(StackContext sctx, PValue[] args)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");

            if (args == null || args.Length < 1 || args[0] == null || args[0].IsNull)
                return PType.Null;

            var iargs = make_tailcall(sctx, args);

            return args[0].IndirectCall(sctx, iargs.ToArray());
        }

        public override StackContext CreateStackContext(StackContext sctx, PValue[] args)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");

            if (args == null || args.Length < 1 || args[0] == null || args[0].IsNull)
                return new NullContext(sctx);

            var iargs = make_tailcall(sctx, args);

            return Call.CreateStackContext(sctx, args[0], iargs.ToArray());
        }

        private static List<PValue> make_tailcall(StackContext sctx, PValue[] args)
        {
            var iargs = Call.FlattenArguments(sctx, args, 1);

            //remove caller from stack
            var stack = sctx.ParentEngine.Stack;
            var node = stack.FindLast(sctx);
            if (node == null)
            {
                throw new PrexoniteException(
                    string.Format("{0} only works on the interpreted stack.", Engine.Call_TailAlias));
            }
            stack.Remove(node);
            return iargs;
        }

        #region Partial application via call\star

        private readonly PartialTailCall _partial = new PartialTailCall();

        public PartialTailCall Partial
        {
            [DebuggerStepThrough]
            get { return _partial; }
        }

        public class PartialTailCall : PartialCallWrapper
        {
            protected PartialTailCall(string alias, string callImplementationId,
                SymbolInterpretations callImplementetaionInterpretation)
                : base(alias, new SymbolEntry(SymbolInterpretations.Command, Alias, null))
            {
            }

            public PartialTailCall()
                : this(Engine.Call_TailAlias, Alias, SymbolInterpretations.Command)
            {
            }

            protected override void DoExpand(MacroContext context)
            {
                _specifyDeficiency(context);

                base.DoExpand(context);
            }

            private static void _specifyDeficiency(MacroContext context)
            {
                context.Function.Meta[PFunction.VolatileKey] = true;
                MetaEntry deficiency;
                if (!context.Function.Meta.TryGetValue(PFunction.DeficiencyKey, out deficiency) ||
                    deficiency.Text == "")
                    context.Function.Meta[PFunction.DeficiencyKey] = string.Format("Uses {0}.",
                        Engine.Call_TailAlias);
            }
        }

        #endregion
    }
}