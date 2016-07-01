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
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using JetBrains.Annotations;

namespace Prexonite.Compiler.Ast
{
    /// <summary>
    ///     An AST expression node for optimized string concatenation.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This node get's created as a replacement for nodes created by <see cref = "IAstFactory.BinaryOperation" /> with string operands.
    ///     </para>
    /// </remarks>
    public class AstStringConcatenation : AstExpr,
                                          IAstHasExpressions
    {
        [NotNull]
        private readonly AstGetSet _simpleConcatPrototype;
        [NotNull]
        private readonly AstGetSet _multiConcatPrototype;

        /// <summary>
        ///     The list of arguments for the string concatenation.
        /// </summary>
        [NotNull] private readonly List<AstExpr> _arguments = new List<AstExpr>();

        /// <summary>
        ///     Creates a new AstStringConcatenation AST node.
        /// </summary>
        /// <param name="position">The portion of source code that spawned this AST node.</param>
        /// <param name="simpleConcatPrototype">A prototype of the call that implements the simple string concatenation (two operands).</param>
        /// <param name="multiConcatPrototype">A prototype of the call that implements the multi-string concatenation (more than two operands).</param>
        /// <param name = "arguments">A list of expressions to be added to the <see cref = "_arguments" /> list.</param>
        [DebuggerNonUserCode]
        public AstStringConcatenation(ISourcePosition position, AstGetSet simpleConcatPrototype, AstGetSet multiConcatPrototype, params AstExpr[] arguments)
            : base(position)
        {
            if (simpleConcatPrototype == null)
                throw new ArgumentNullException(nameof(simpleConcatPrototype));
            if (multiConcatPrototype == null)
                throw new ArgumentNullException(nameof(multiConcatPrototype));
            
            if (arguments == null)
                arguments = new AstExpr[] {};

            _arguments.AddRange(arguments);
            _simpleConcatPrototype = simpleConcatPrototype;
            _multiConcatPrototype = multiConcatPrototype;
        }

        #region IAstHasExpressions Members

        public AstExpr[] Expressions
        {
            get { return _arguments.ToArray(); }
        }

        /// <summary>
        ///     The list of arguments for the string concatenation.
        /// </summary>
        public List<AstExpr> Arguments
        {
            get { return _arguments; }
        }

        #endregion

        /// <summary>
        ///     Emits code for the AstStringConcatenation node.
        /// </summary>
        /// <param name = "target">The target to which to write the code to.</param>
        /// <param name="stackSemantics">The stack semantics with which to emit code. </param>
        /// <remarks>
        ///     <para>
        ///         AstStringConcatenation tries to find the most efficient way to concatenate strings. StringBuilders are actually slower when concatenating only two arguments.
        ///     </para>
        ///     <para>
        ///         <list type = "table">
        ///             <listheader>
        ///                 <term>Arguments</term>
        ///                 <description>Emitted code</description>
        ///             </listheader>
        ///             <item>
        ///                 <term>0</term>
        ///                 <description><c><see cref = "OpCode.ldc_string">ldc.string</see> ""</c> (Empty string)</description>
        ///             </item>
        ///             <item>
        ///                 <term>1</term>
        ///                 <description>Just that argument and, unless it is a <see cref = "AstConstant">string constant</see>, a call to <c>ToString</c>.</description>
        ///             </item>
        ///             <item>
        ///                 <term>2</term>
        ///                 <description>Concatenation using the Addition (<c><see cref = "OpCode.add">add</see></c>) operator.</description>
        ///             </item>
        ///             <item>
        ///                 <term>n</term>
        ///                 <description>A call to the <c><see cref = "Prexonite.Engine.ConcatenateAlias">concat</see></c> command.</description>
        ///             </item>
        ///         </list>
        ///     </para>
        /// </remarks>
        protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
        {
            if (_arguments.Count > 2)
            {
                var call = _multiConcatPrototype.GetCopy();
                call.Arguments.AddRange(Arguments);
                call.EmitCode(target, stackSemantics);
            }
            else if (_arguments.Count >= 2)
            {
                var call = _simpleConcatPrototype.GetCopy();
                call.Arguments.AddRange(Arguments);
                call.EmitCode(target, stackSemantics);
            }
            else if (_arguments.Count == 1)
            {
                if (stackSemantics == StackSemantics.Value)
                {
                    _arguments[0].EmitValueCode(target);

                    AstConstant constant;
                    if ((constant = _arguments[0] as AstConstant) != null &&
                        !(constant.Constant is string))
                        target.EmitGetCall(Position, 1, "ToString");
                }
                else
                {
                    _arguments[0].EmitEffectCode(target);
                }
            }
            else if (_arguments.Count == 0)
            {
                if(stackSemantics == StackSemantics.Value)
                    target.EmitConstant(Position, "");
            }
        }

        #region AstExpr Members

        /// <summary>
        ///     Tries to optimize the AstStringConcatenation node.
        /// </summary>
        /// <param name = "target">The context in which to perform the optimization.</param>
        /// <param name = "expr">A possible replacement for the called node.</param>
        /// <returns>True, if <paramref name = "expr" /> contains a replacement for the called node. 
        ///     False otherwise.</returns>
        /// <remarks>
        ///     <para>
        ///         Note that <c>false</c> as the return value doesn't mean that the node cannot be 
        ///         optimized. It just cannot be replaced by <paramref name = "expr" />.
        ///     </para>
        ///     <para>
        ///         Also, <paramref name = "expr" /> is only defined if the method call returns <c>true</c>. Don't use it otherwise.
        ///     </para>
        /// </remarks>
        public override bool TryOptimize(CompilerTarget target, out AstExpr expr)
        {
            _OptimizeInternal(target);

            AstConstant collapsed;
            if (Arguments.Count == 1 && (collapsed = Arguments[0] as AstConstant) != null)
            {
                expr = collapsed.Constant is string
                    ? (AstExpr) collapsed
                    : new AstGetSetMemberAccess(File, Line, Column, collapsed, "ToString");
            }
            else
            {
                expr = null;
            }

            return expr != null;
        }

        internal void _OptimizeInternal(CompilerTarget target)
        {
//Optimize arguments
            foreach (var arg in _arguments.ToArray())
            {
                if (arg == null)
                    throw new PrexoniteException(
                        "Invalid (null) argument in StringConcat node (" + ToString() +
// ReSharper disable ConditionIsAlwaysTrueOrFalse
                            ") detected.");
// ReSharper restore ConditionIsAlwaysTrueOrFalse
                var oArg = _GetOptimizedNode(target, arg);
                if (!ReferenceEquals(oArg, arg))
                {
                    var idx = _arguments.IndexOf(arg);
                    _arguments.Insert(idx, oArg);
                    _arguments.RemoveAt(idx + 1);
                }
            }

            //Expand embedded concats argument list
            var argumentArray = _arguments.ToArray();
            for (var i = 0; i < argumentArray.Length; i++)
            {
                var argument = argumentArray[i];
                var concat = argument as AstStringConcatenation;

                if (concat != null)
                {
                    _arguments.RemoveAt(i); //Remove embedded concat
                    _arguments.InsertRange(i, concat._arguments); //insert it's arguments instead
                }
            }

            //Try to shorten argument list
            var nlst = new List<AstExpr>();
            string last = null;
            var buffer = new StringBuilder();
            foreach (var e in _arguments)
            {
                string current;
                var currConst = e as AstConstant;
                if (currConst != null)
                    current = currConst.ToPValue(target).CallToString(target.Loader);
                else
                    current = null;

                if (current != null)
                {
                    //Drop empty strings
                    if (current.Length == 0)
                        continue;

                    buffer.Append(current);
                }
                else //current == null
                {
                    if (last != null)
                    {
                        nlst.Add(new AstConstant(File, Line, Column, buffer.ToString()));
                        buffer.Length = 0;
                    }

                    nlst.Add(e);
                }

                last = current;
            }
            if (last != null)
            {
                nlst.Add(new AstConstant(File, Line, Column, buffer.ToString()));
                buffer.Length = 0;
            }

            _arguments.Clear();
            _arguments.AddRange(nlst);
        }

        #endregion
    }
}