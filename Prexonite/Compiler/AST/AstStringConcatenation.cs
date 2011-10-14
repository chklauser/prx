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

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Prexonite.Commands.Core.Operators;
using Prexonite.Types;

namespace Prexonite.Compiler.Ast
{
    /// <summary>
    ///     An AST expression node for optimized string concatenation.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This node get's created as a replacement for <see cref = "AstBinaryOperator" /> nodes with string operands.
    ///     </para>
    /// </remarks>
    public class AstStringConcatenation : AstNode,
                                          IAstExpression,
                                          IAstHasExpressions
    {
        public SymbolInterpretations OperatorInterpretation { get; set; }

        public string OperatorId { get; set; }

        /// <summary>
        ///     The list of arguments for the string concatenation.
        /// </summary>
        public List<IAstExpression> Arguments = new List<IAstExpression>();

        /// <summary>
        ///     Creates a new AstStringConcatenation AST node.
        /// </summary>
        /// <param name = "file">The file that caused this node to be created.</param>
        /// <param name = "line">The line that caused this node to be created.</param>
        /// <param name = "column">The column that caused this node to be created.</param>
        /// <param name = "arguments">A list of expressions to be added to the <see cref = "Arguments" /> list.</param>
        [DebuggerNonUserCode]
        public AstStringConcatenation(
            string file, int line, int column, SymbolInterpretations operatorImplementation,
            string operatorId, params IAstExpression[] arguments)
            : base(file, line, column)
        {
            if (arguments == null)
                arguments = new IAstExpression[] {};

            Arguments.AddRange(arguments);
            OperatorId = operatorId;
            OperatorInterpretation = operatorImplementation;
        }

        internal AstStringConcatenation Create(Parser p, params IAstExpression[] arguments)
        {
            string id;
            var interpretation = Resolve(p, OperatorNames.Prexonite.Addition, out id);
            return new AstStringConcatenation(p.scanner.File, p.t.line, p.t.col, interpretation, id,
                arguments);
        }

        #region IAstHasExpressions Members

        public IAstExpression[] Expressions
        {
            get { return Arguments.ToArray(); }
        }

        #endregion

        /// <summary>
        ///     Emits code for the AstStringConcatenation node.
        /// </summary>
        /// <param name = "target">The target to which to write the code to.</param>
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
        protected override void DoEmitCode(CompilerTarget target)
        {
            if (Arguments.Count > 2
                && OperatorInterpretation == SymbolInterpretations.Command
                    && OperatorId == Addition.DefaultAlias)
            {
                var call = new AstGetSetSymbol(File, Line, Column, PCall.Get,
                    Engine.ConcatenateAlias,
                    SymbolInterpretations.Command);
                call.Arguments.AddRange(Arguments);
                call.EmitCode(target);
            }
            else if (Arguments.Count >= 2)
            {
                var op = Arguments.Skip(1).Aggregate(Arguments[0],
                    (aggregate, right) =>
                        new AstBinaryOperator(File, Line, Column, aggregate, BinaryOperator.Addition,
                            right,
                            OperatorInterpretation, OperatorId));
                op.EmitCode(target);
            }
            else if (Arguments.Count == 1)
            {
                Arguments[0].EmitCode(target);

                AstConstant constant;
                if ((constant = Arguments[0] as AstConstant) != null &&
                    !(constant.Constant is string))
                    target.EmitGetCall(this, 1, "ToString");
            }
            else if (Arguments.Count == 0)
            {
                target.EmitConstant(this, "");
            }
        }

        #region IAstExpression Members

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
        public bool TryOptimize(CompilerTarget target, out IAstExpression expr)
        {
            //Optimize arguments
            foreach (var arg in Arguments.ToArray())
            {
                if (arg == null)
                    throw new PrexoniteException(
                        "Invalid (null) argument in StringConcat node (" + ToString() +
                            ") detected at position " + Arguments.IndexOf(arg) + ".");
                var oArg = _GetOptimizedNode(target, arg);
                if (!ReferenceEquals(oArg, arg))
                {
                    var idx = Arguments.IndexOf(arg);
                    Arguments.Insert(idx, oArg);
                    Arguments.RemoveAt(idx + 1);
                }
            }

            expr = null;

            //Expand embedded concats argument list
            var argumentArray = Arguments.ToArray();
            for (var i = 0; i < argumentArray.Length; i++)
            {
                var argument = argumentArray[i];
                var concat = argument as AstStringConcatenation;

                if (concat != null)
                {
                    Arguments.RemoveAt(i); //Remove embedded concat
                    Arguments.InsertRange(i, concat.Arguments); //insert it's arguments instead
                }
            }

            //Try to shorten argument list
            var nlst = new List<IAstExpression>();
            string last = null;
            var buffer = new StringBuilder();
            foreach (var e in Arguments)
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

            Arguments = nlst;

            AstConstant collapsed;
            if (nlst.Count == 1 && (collapsed = nlst[0] as AstConstant) != null)
            {
                expr = collapsed.Constant is string
                    ? (IAstExpression) collapsed
                    : new AstGetSetMemberAccess(File, Line, Column, collapsed, "ToString");
            }

            return expr != null;
        }

        #endregion
    }
}