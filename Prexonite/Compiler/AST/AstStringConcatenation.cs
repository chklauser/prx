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

using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Prexonite;

namespace Prexonite.Compiler.Ast
{
    /// <summary>
    /// An AST expression node for optimized string concatenation.
    /// </summary>
    /// <remarks>
    /// <para>
    ///     This node get's created as a replacement for <see cref="AstBinaryOperator"/> nodes with string operands.
    /// </para></remarks>
    public class AstStringConcatenation : AstNode,
                                          IAstExpression,
                                          IAstHasExpressions
    {
        /// <summary>
        /// The list of arguments for the string concatenation.
        /// </summary>
        public List<IAstExpression> Arguments = new List<IAstExpression>();

        /// <summary>
        /// Creates a new AstStringConcatenation AST node.
        /// </summary>
        /// <param name="file">The file that caused this node to be created.</param>
        /// <param name="line">The line that caused this node to be created.</param>
        /// <param name="column">The column that caused this node to be created.</param>
        /// <param name="arguments">A list of expressions to be added to the <see cref="Arguments"/> list.</param>
        [DebuggerNonUserCode]
        public AstStringConcatenation(
            string file, int line, int column, params IAstExpression[] arguments)
            : base(file, line, column)
        {
            if (arguments == null)
                arguments = new IAstExpression[] {};

            Arguments.AddRange(arguments);
        }

        [DebuggerNonUserCode]
        internal AstStringConcatenation(Parser p, params IAstExpression[] arguments)
            : base(p)
        {
            if (arguments == null)
                arguments = new IAstExpression[] {};

            Arguments.AddRange(arguments);
        }

        #region IAstHasExpressions Members

        public IAstExpression[] Expressions
        {
            get { return Arguments.ToArray(); }
        }

        #endregion

        /// <summary>
        /// Emits code for the AstStringConcatenation node.
        /// </summary>
        /// <param name="target">The target to which to write the code to.</param>
        /// <remarks>
        /// <para>
        ///     AstStringConcatenation tries to find the most efficient way to concatenate strings. StringBuilders are actually slower when concatenating only two arguments.
        /// </para>
        /// <para>
        ///     <list type="table">
        ///         <listheader>
        ///             <term>Arguments</term>
        ///             <description>Emitted code</description>
        ///         </listheader>
        ///         <item>
        ///             <term>0</term>
        ///             <description><c><see cref="OpCode.ldc_string">ldc.string</see> ""</c> (Empty string)</description>
        ///         </item>
        ///         <item>
        ///             <term>1</term>
        ///             <description>Just that argument and, unless it is a <see cref="AstConstant">string constant</see>, a call to <c>ToString</c>.</description>
        ///         </item>
        ///         <item>
        ///             <term>2</term>
        ///             <description>Concatenation using the Addition (<c><see cref="OpCode.add">add</see></c>) operator.</description>
        ///         </item>
        ///         <item>
        ///             <term>n</term>
        ///             <description>A call to the <c><see cref="Engine.ConcatenateAlias">concat</see></c> command.</description>
        ///         </item>
        ///     </list>
        /// </para>
        /// </remarks>
        public override void EmitCode(CompilerTarget target)
        {
            foreach (IAstExpression arg in Arguments)
                arg.EmitCode(target);

            if (Arguments.Count > 2)
                target.EmitCommandCall(Arguments.Count, Engine.ConcatenateAlias);
            else if (Arguments.Count == 2)
                AstBinaryOperator.EmitOperator(target, BinaryOperator.Addition);
            else if (Arguments.Count == 1)
            {
                AstConstant constant;
                if ((constant = Arguments[0] as AstConstant) != null &&
                    !(constant.Constant is string))
                    target.EmitGetCall(1, "ToString");
            }
            else if (Arguments.Count == 0)
                target.EmitConstant("");
        }

        #region IAstExpression Members

        /// <summary>
        /// Tries to optimize the AstStringConcatenation node.
        /// </summary>
        /// <param name="target">The context in which to perform the optimization.</param>
        /// <param name="expr">A possible replacement for the called node.</param>
        /// <returns>True, if <paramref name="expr"/> contains a replacement for the called node. 
        /// False otherwise.</returns>
        /// <remarks>
        /// <para>
        ///     Note that <c>false</c> as the return value doesn't mean that the node cannot be 
        ///     optimized. It just cannot be replaced by <paramref name="expr"/>.
        /// </para>
        /// <para>
        ///     Also, <paramref name="expr"/> is only defined if the method call returns <c>true</c>. Don't use it otherwise.
        /// </para>
        /// </remarks>
        public bool TryOptimize(CompilerTarget target, out IAstExpression expr)
        {
            //Optimize arguments
            IAstExpression oArg;
            foreach (IAstExpression arg in Arguments.ToArray())
            {
                if (arg == null)
                    throw new PrexoniteException(
                        "Invalid (null) argument in StringConcat node (" + ToString() +
                        ") detected at position " + Arguments.IndexOf(arg) + ".");
                oArg = GetOptimizedNode(target, arg);
                if (!ReferenceEquals(oArg, arg))
                {
                    int idx = Arguments.IndexOf(arg);
                    Arguments.Insert(idx, oArg);
                    Arguments.RemoveAt(idx + 1);
                }
            }

            expr = null;

            //Expand embedded concats argument list
            IAstExpression[] argumentArray = Arguments.ToArray();
            for (int i = 0; i < argumentArray.Length; i++)
            {
                IAstExpression argument = argumentArray[i];
                AstStringConcatenation concat = argument as AstStringConcatenation;

                if (concat != null)
                {
                    Arguments.RemoveAt(i); //Remove embedded concat
                    Arguments.InsertRange(i, concat.Arguments); //insert it's arguments instead
                }
            }

            //Try to shorten argument list
            List<IAstExpression> nlst = new List<IAstExpression>();
            string last = null;
            StringBuilder buffer = new StringBuilder();
            foreach (IAstExpression e in Arguments)
            {
                string current;
                AstConstant currConst = e as AstConstant;
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
            return expr != null;
        }

        #endregion
    }
}