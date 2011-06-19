/*
 * Prx, a standalone command line interface to the Prexonite scripting engine.
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
using System.Diagnostics;
using System.Linq;
using Prexonite.Commands.List;
using Prexonite.Compiler;
using Prexonite.Compiler.Ast;
using Prexonite.Compiler.Cil;
using Prexonite.Compiler.Macro;
using Prexonite.Types;

namespace Prexonite.Commands.Core
{
    /// <summary>
    /// Implementation of (ref f, arg1, arg2, arg3, ..., argn) => f(arg1, arg2, arg3, ..., argn);
    /// </summary>
    /// <remarks>
    /// <para>
    ///     Returns null if no callable object is passed.
    /// </para>
    /// <para>
    ///     Uses the <see cref="IIndirectCall"/> interface.
    /// </para>
    /// </remarks>
    /// <seealso cref="IIndirectCall"/>
    public sealed class Call : StackAwareCommand, ICilCompilerAware
    {
        private Call()
        {
        }

        public const string Alias = @"call\perform";

        private static readonly Call _instance = new Call();

        public static Call Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// Implementation of (ref f, arg1, arg2, arg3, ..., argn) => f(arg1, arg2, arg3, ..., argn);
        /// </summary>
        /// <remarks>
        /// <para>
        ///     Returns null if no callable object is passed.
        /// </para>
        /// <para>
        ///     Uses the <see cref="IIndirectCall"/> interface.
        /// </para>
        /// <para>
        ///     Wrap Lists in other lists, if you want to pass them without being unfolded: 
        /// <code>
        /// function main()
        /// {   var myList = [1, 2, 3];
        ///     var f = xs => xs.Count;
        ///     print( call(f, [ myList ]) );
        /// }
        /// 
        /// //Prints "3"
        /// </code>
        /// </para>
        /// </remarks>
        /// <seealso cref="IIndirectCall"/>
        /// <param name="sctx">The stack context in which to call the callable argument.</param>
        /// <param name="args">A list of the form [ ref f, arg1, arg2, arg3, ..., argn].<br />
        /// Lists and coroutines are expanded.</param>
        /// <returns>The result returned by <see cref="IIndirectCall.IndirectCall"/> or PValue Null if no callable object has been passed.</returns>
        public static PValue RunStatically(StackContext sctx, PValue[] args)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (args == null || args.Length == 0 || args[0] == null)
                return PType.Null.CreatePValue();

            var iargs = FlattenArguments(sctx, args, 1);

            return args[0].IndirectCall(sctx, iargs.ToArray());
        }

        /// <summary>
        /// Implementation of (ref f, arg1, arg2, arg3, ..., argn) => f(arg1, arg2, arg3, ..., argn);
        /// </summary>
        /// <remarks>
        /// <para>
        ///     Returns null if no callable object is passed.
        /// </para>
        /// <para>
        ///     Uses the <see cref="IIndirectCall"/> interface.
        /// </para>
        /// <para>
        ///     Wrap Lists in other lists, if you want to pass them without being unfolded: 
        /// <code>
        /// function main()
        /// {   var myList = [1, 2, 3];
        ///     var f = xs => xs.Count;
        ///     print( call(f, [ myList ]) );
        /// }
        /// 
        /// //Prints "3"
        /// </code>
        /// </para>
        /// </remarks>
        /// <seealso cref="IIndirectCall"/>
        /// <param name="sctx">The stack context in which to call the callable argument.</param>
        /// <param name="args">A list of the form [ ref f, arg1, arg2, arg3, ..., argn].<br />
        /// Lists and coroutines are expanded.</param>
        /// <returns>The result returned by <see cref="IIndirectCall.IndirectCall"/> or PValue Null if no callable object has been passed.</returns>
        public override PValue Run(StackContext sctx, PValue[] args)
        {
            return RunStatically(sctx, args);
        }

        [DebuggerStepThrough]
        public static List<PValue> FlattenArguments(StackContext sctx, PValue[] args)
        {
            return FlattenArguments(sctx, args, 0);
        }

        /// <summary>
        /// Takes an argument list and injects elements of top-level lists into that argument list.
        /// </summary>
        /// <param name="sctx">The stack context in which to convert enumerables.</param>
        /// <param name="args">The raw list of arguments to process.</param>
        /// <param name="offset">The offset at which to start processing.</param>
        /// <returns>A copy of the argument list with top-level lists expanded.</returns>
        public static List<PValue> FlattenArguments(StackContext sctx, PValue[] args, int offset)
        {
            if (args == null)
                args = new PValue[] {};
            var iargs = new List<PValue>();
            for (var i = offset; i < args.Length; i++)
            {
                var arg = args[i];
                var folded = Map._ToEnumerable(sctx, arg);
                if (folded == null)
                    iargs.Add(arg);
                else
                    iargs.AddRange(folded);
            }
            return iargs;
        }

        public override StackContext CreateStackContext(StackContext sctx, PValue[] args)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (args == null || args.Length == 0 || args[0] == null || args[0].IsNull)
                return new NullContext(sctx);

            var iargs = FlattenArguments(sctx, args, 1);

            var callable = args[0];
            return CreateStackContext(sctx, callable, iargs.ToArray());
        }

        public static StackContext CreateStackContext(StackContext sctx, PValue callable, PValue[] args)
        {
            var sa = callable.Value as IStackAware;
            if (callable.Type is ObjectPType && sa != null)
                return sa.CreateStackContext(sctx, args);
            else
                return new IndirectCallContext(sctx, callable, args);
        }

        #region ICilCompilerAware Members

        /// <summary>
        /// Asses qualification and preferences for a certain instruction.
        /// </summary>
        /// <param name="ins">The instruction that is about to be compiled.</param>
        /// <returns>A set of <see cref="CompilationFlags"/>.</returns>
        CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
        {
            return CompilationFlags.PrefersRunStatically;
        }

        /// <summary>
        /// Provides a custom compiler routine for emitting CIL byte code for a specific instruction.
        /// </summary>
        /// <param name="state">The compiler state.</param>
        /// <param name="ins">The instruction to compile.</param>
        void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region Macro for partial application

        public class Partial : PartialMacroCommand
        {
            public const string Alias = Engine.CallAlias;

            #region Singleton pattern

            private static readonly Partial _instance = new Partial();

            public static Partial Instance
            {
                get { return _instance; }
            }

            private Partial() : base(Alias)
            {
            }

            #endregion

            #region Overrides of MacroCommand

            private static bool _hasPlaceholder(IAstExpression expr)
            {
                var lit = expr as AstListLiteral;
                return expr.IsPlaceholder() || (lit != null && lit.CheckForPlaceholders());
            }

            protected override void DoExpand(MacroContext context)
            {
                if(context.Invocation.Arguments.Count == 0)
                {
                    // Call with no arguments returns null.
                    // The macro system will supply that null.
                    return;
                }

                var p = context.Invocation.Arguments[0] as AstPlaceholder;
                if(p != null && (p.Index.GetValueOrDefault(0) == 0))
                {
                    // call(?0) ⇒ call\perform(?0)

                    var cp = new AstGetSetSymbol(context.Invocation.File, context.Invocation.Line,
                        context.Invocation.Column, context.Invocation.Call, Call.Alias,
                        SymbolInterpretations.Command);
                    cp.Arguments.Add(new AstPlaceholder(context.Invocation.File, context.Invocation.Line, context.Invocation.Column,0));
                    context.Block.Expression = cp;
                    return;
                }

                if(!context.Invocation.Arguments.Any(_hasPlaceholder))
                {
                    // no placeholders, invoke call\perform directly

                    var call = context.CreateGetSetSymbol(SymbolInterpretations.Command,
                        context.Call, Call.Alias, context.Invocation.Arguments.ToArray());
                    context.Block.Expression = call;
                    return;
                }

                var inv = new AstMacroInvocation(context.Invocation.File, context.Invocation.Line,
                    context.Invocation.Column,
                    Compiler.Macro.Commands.CallStar.Instance.Id,
                    SymbolInterpretations.MacroCommand);

                // Protected the first two arguments
                inv.Arguments.Add(context.CreateConstant(2));

                // Indicate the kind of call by passing `call(?)`, a partial application of call
                var paCall = context.CreateGetSetSymbol(SymbolInterpretations.Command,
                    context.Invocation.Call, Engine.CallAlias,
                    new AstPlaceholder(context.Invocation.File, context.Invocation.Line,
                        context.Invocation.Column, 0));
                inv.Arguments.Add(paCall);

                // Pass all the other arguments through
                inv.Arguments.AddRange(context.Invocation.Arguments);

                context.Block.Expression = inv;
            }

            #endregion

            #region Overrides of PartialMacroCommand

            protected override bool DoExpandPartialApplication(MacroContext context)
            {
                DoExpand(context);
                return true;
            }

            #endregion
        }

        #endregion
    }
}