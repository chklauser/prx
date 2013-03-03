// Prexonite
// 
// Copyright (c) 2013, Christian Klauser
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
using System.Linq;
using Prexonite.Commands.List;
using Prexonite.Compiler;
using Prexonite.Compiler.Ast;
using Prexonite.Compiler.Macro;
using Prexonite.Compiler.Macro.Commands;
using Prexonite.Modular;
using Prexonite.Types;

namespace Prexonite.Commands.Core
{
    /// <summary>
    ///     Implementation of (obj, [isSet, ] id, arg1, arg2, arg3, ..., argn) => obj.id(arg1, arg2, arg3, ..., argn);
    /// </summary>
    public sealed class Call_Member : PCommand
    {
        #region Singleton

        private Call_Member()
        {
        }

        private static Call_Member _instance = new Call_Member();

        public static Call_Member Instance
        {
            get { return _instance; }
        }

        #endregion

        public const string Alias = @"call\member\perform";

        /// <summary>
        ///     Implementation of (obj, [isSet, ] id, arg1, arg2, arg3, ..., argn) ⇒ obj.id(arg1, arg2, arg3, ..., argn);
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Wrap Lists in other lists, if you want to pass them without being unfolded: 
        ///         <code>
        ///             function main()
        ///             {   var myList = [1, 2];
        ///             var obj = "{1}hell{0}";
        ///             print( call\member(obj, "format",  [ myList ]) );
        ///             }
        /// 
        ///             //Prints "2hell1"
        ///         </code>
        ///     </para>
        /// </remarks>
        /// <param name = "sctx">The stack context in which to call the callable argument.</param>
        /// <param name = "args">A list of the form [ obj, id, arg1, arg2, arg3, ..., argn].<br />
        ///     Lists and coroutines are expanded.</param>
        /// <returns>The result returned by the member call.</returns>
        public override PValue Run(StackContext sctx, PValue[] args)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (args == null || args.Length < 2 || args[0] == null)
                throw new ArgumentException(
                    "The command callmember has the signature(obj, [isSet,] id [, arg1, arg2,...,argn]).");

            var isSet = false;
            string id;
            var i = 2;

            if (args[1].Type == PType.Bool && args.Length > 2)
            {
                isSet = (bool) args[1].Value;
                id = args[i++].CallToString(sctx);
            }
            else
            {
                id = args[1].CallToString(sctx);
            }


            var iargs = new PValue[args.Length - i];
            Array.Copy(args, i, iargs, 0, iargs.Length);

            return Run(sctx, args[0], isSet, id, iargs);
        }

        /// <summary>
        ///     Implementation of (obj, id, arg1, arg2, arg3, ..., argn) => obj.id(arg1, arg2, arg3, ..., argn);
        /// </summary>
        /// <param name = "sctx">The stack context in which to call the member of <paramref name = "obj" />.</param>
        /// <param name = "obj">The obj to call.</param>
        /// <param name = "id">The id of the member to call.</param>
        /// <param name = "args">The array of arguments to pass to the member call.<br />
        ///     Lists and coroutines are expanded.</param>
        /// <returns>The result returned by the member call.</returns>
        /// <exception cref = "ArgumentNullException"><paramref name = "sctx" /> is null.</exception>
        public PValue Run(StackContext sctx, PValue obj, string id, params PValue[] args)
        {
            return Run(sctx, obj, false, id, args);
        }

        /// <summary>
        ///     Implementation of (obj, id, arg1, arg2, arg3, ..., argn) => obj.id(arg1, arg2, arg3, ..., argn);
        /// </summary>
        /// <param name = "sctx">The stack context in which to call the member of <paramref name = "obj" />.</param>
        /// <param name = "obj">The obj to call.</param>
        /// <param name = "isSet">Indicates whether to perform a Set-call.</param>
        /// <param name = "id">The id of the member to call.</param>
        /// <param name = "args">The array of arguments to pass to the member call.<br />
        ///     Lists and coroutines are expanded.</param>
        /// <returns>The result returned by the member call.</returns>
        /// <exception cref = "ArgumentNullException"><paramref name = "sctx" /> is null.</exception>
        public PValue Run(StackContext sctx, PValue obj, bool isSet, string id, params PValue[] args)
        {
            if (obj == null)
                return PType.Null.CreatePValue();
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (args == null)
                args = new PValue[] {};

            var iargs = new List<PValue>();
            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                var folded = Map._ToEnumerable(sctx, arg);
                if (folded == null)
                    iargs.Add(arg);
                else
                    iargs.AddRange(folded);
            }

            return obj.DynamicCall(sctx, iargs.ToArray(), isSet ? PCall.Set : PCall.Get, id);
        }

        #region Partial application via call\star

        private readonly PartialMemberCall _partial = new PartialMemberCall();

        public PartialMemberCall Partial
        {
            [DebuggerStepThrough]
            get { return _partial; }
        }

        public class PartialMemberCall : PartialCallWrapper
        {
            protected PartialMemberCall(string alias, string callImplementationId,
                SymbolInterpretations callImplementetaionInterpretation)
                : base(alias, EntityRef.Command.Create(Alias))
            {
            }

            public PartialMemberCall()
                : this(Engine.Call_MemberAlias, Alias, SymbolInterpretations.Command)
            {
            }

            protected override IEnumerable<AstExpr> GetCallArguments(MacroContext context)
            {
                var argv = context.Invocation.Arguments;
                return
                    argv.Take(1).Append(_getIsSetExpr(context)).Append(argv.Skip(1));
            }

            private static AstExpr _getIsSetExpr(MacroContext context)
            {
                return context.CreateConstant(context.Invocation.Call == PCall.Set);
            }

            protected override AstGetSet GetTrivialPartialApplication(MacroContext context)
            {
                var pa = base.GetTrivialPartialApplication(context);
                pa.Arguments.Insert(1, _getIsSetExpr(context));
                return pa;
            }

            protected override int GetPassThroughArguments(MacroContext context)
            {
                return 3;
            }
        }

        #endregion
    }
}