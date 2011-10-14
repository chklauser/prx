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

using System;
using System.Collections.Generic;
using System.Linq;
using Prexonite.Commands.Core;
using Prexonite.Types;
using Debug = System.Diagnostics.Debug;
using NotSupportedException = Prexonite.Commands.Concurrency.NotSupportedException;

namespace Prexonite.Compiler.Ast
{
    public abstract class AstLazyLogical : AstNode,
                                           IAstExpression,
                                           IAstHasExpressions
    {
        private readonly LinkedList<IAstExpression> _conditions = new LinkedList<IAstExpression>();

        internal AstLazyLogical(
            Parser p, IAstExpression leftExpression, IAstExpression rightExpression)
            : this(p.scanner.File, p.t.line, p.t.col, leftExpression, rightExpression)
        {
        }

        protected AstLazyLogical(
            string file,
            int line,
            int column,
            IAstExpression leftExpression,
            IAstExpression rightExpression)
            : base(file, line, column)
        {
            AddExpression(leftExpression);
            AddExpression(rightExpression);
        }

        #region IAstHasExpressions Members

        public IAstExpression[] Expressions
        {
            get
            {
                var len = _conditions.Count;
                var ary = new IAstExpression[len];
                var i = 0;
                foreach (var condition in _conditions)
                    ary[i++] = condition;
                return ary;
            }
        }

        public LinkedList<IAstExpression> Conditions
        {
            get { return _conditions; }
        }

        #endregion

        public void AddExpression(IAstExpression expr)
        {
            //Flatten hierarchy
            var lazy = expr as AstLazyLogical;
            if (lazy != null && lazy.GetType() == GetType())
            {
                foreach (var cond in lazy._conditions)
                    AddExpression(cond);
            }
            else
            {
                _conditions.AddLast(expr);
            }
        }

        public void EmitCode(CompilerTarget target, string trueLabel, string falseLabel)
        {
            foreach (var condition in Conditions)
            {
                if (((AstNode) condition).CheckForPlaceholders())
                {
                    target.Loader.ReportSemanticError(condition.Line, condition.Column,
                        "Partial applications of logical statements must be either pure and-chains or pure or-chains.");
                    target.EmitJump(this, trueLabel);
                    return;
                }
            }


            DoEmitCode(target, trueLabel, falseLabel);
        }

        protected abstract void DoEmitCode(CompilerTarget target, string trueLabel,
            string falseLabel);

        /// <summary>
        ///     Emit the jump code for an if-like condition (jump if true). Recognizes and takes advantage of lazy logical expressions.
        /// </summary>
        /// <param name = "target">The compiler target to emit code to.</param>
        /// <param name = "cond">The condition of the jump.</param>
        /// <param name = "targetLabel">The target of the conditional jump.</param>
        public static void EmitJumpIfCondition(
            CompilerTarget target, IAstExpression cond, string targetLabel)
        {
            if (cond == null)
                throw new ArgumentNullException("cond", "Condition may not be null.");
            if (target == null)
                throw new ArgumentNullException("target", "Compiler target may not be null.");
            if (String.IsNullOrEmpty(targetLabel))
                throw new ArgumentException(
                    "targetLabel may neither be null nor empty.", "targetLabel");
            var logical = cond as AstLazyLogical;
            if (logical != null)
            {
                var continueLabel = "Continue\\Lazy\\" + Guid.NewGuid().ToString("N");
                logical.EmitCode(target, targetLabel, continueLabel);
                target.EmitLabel(cond, continueLabel);
            }
            else
            {
                cond.EmitCode(target);
                target.EmitJumpIfTrue(cond, targetLabel);
            }
        }

        public static void EmitJumpCondition(
            CompilerTarget target,
            IAstExpression cond,
            string targetLabel,
            bool isPositive)
        {
            if (isPositive)
                EmitJumpIfCondition(target, cond, targetLabel);
            else
                EmitJumpUnlessCondition(target, cond, targetLabel);
        }

        public static void EmitJumpCondition(
            CompilerTarget target,
            IAstExpression cond,
            string targetLabel,
            string alternativeLabel,
            bool isPositive)
        {
            if (cond == null)
                throw new ArgumentNullException("cond", "Condition may not be null.");
            if (target == null)
                throw new ArgumentNullException("target", "Compiler target may not be null.");
            if (String.IsNullOrEmpty(targetLabel))
                throw new ArgumentException(
                    "targetLabel may neither be null nor empty.", "targetLabel");
            if (String.IsNullOrEmpty(alternativeLabel))
                throw new ArgumentException(
                    "alternativeLabel may neither be null nor empty.", "alternativeLabel");
            var logical = cond as AstLazyLogical;
            if (!isPositive)
            {
                //Invert if needed
                var tmpLabel = alternativeLabel;
                alternativeLabel = targetLabel;
                targetLabel = tmpLabel;
            }
            if (logical != null)
            {
                logical.EmitCode(target, targetLabel, alternativeLabel);
            }
            else
            {
                cond.EmitCode(target);
                target.EmitJumpIfTrue(cond, targetLabel);
                target.EmitJump(cond, alternativeLabel);
            }
        }

        public static void EmitJumpCondition(
            CompilerTarget target,
            IAstExpression cond,
            string targetLabel,
            string alternativeLabel)
        {
            EmitJumpCondition(target, cond, targetLabel, alternativeLabel, true);
        }

        public static void EmitJumpUnlessCondition(
            CompilerTarget target, IAstExpression cond, string targetLabel)
        {
            if (cond == null)
                throw new ArgumentNullException("cond", "Condition may not be null.");
            if (target == null)
                throw new ArgumentNullException("target", "Compiler target may not be null.");
            if (String.IsNullOrEmpty(targetLabel))
                throw new ArgumentException(
                    "targetLabel may neither be null nor empty.", "targetLabel");
            var logical = cond as AstLazyLogical;
            if (logical != null)
            {
                var continueLabel = "Continue\\Lazy\\" + Guid.NewGuid().ToString("N");
                logical.EmitCode(target, continueLabel, targetLabel); //inverted
                target.EmitLabel(cond, continueLabel);
            }
            else
            {
                cond.EmitCode(target);
                target.EmitJumpIfFalse(cond, targetLabel);
            }
        }

        #region Partial application

        public override bool CheckForPlaceholders()
        {
            return this is IAstPartiallyApplicable &&
                (base.CheckForPlaceholders() || Conditions.Any(AstPartiallyApplicable.IsPlaceholder));
        }

        #endregion

        public static IAstExpression CreateConjunction(ISourcePosition position,
            IEnumerable<IAstExpression> clauses)
        {
            var cs = clauses.ToList();

            if (cs.Count == 0)
            {
                return new AstConstant(position.File, position.Line, position.Column, true);
            }
            else if (cs.Count == 1)
            {
                return cs[0];
            }
            else
            {
                var conj = new AstLogicalAnd(position.File, position.Line, position.Column, cs[0],
                    cs[1]);
                foreach (var clause in cs.Skip(2))
                    conj.AddExpression(clause);
                return conj;
            }
        }

        public static IAstExpression CreateDisjunction(ISourcePosition position,
            IEnumerable<IAstExpression> clauses)
        {
            var cs = clauses.ToList();

            if (cs.Count == 0)
            {
                return new AstConstant(position.File, position.Line, position.Column, true);
            }
            else if (cs.Count == 1)
            {
                return cs[0];
            }
            else
            {
                var disj = new AstLogicalOr(position.File, position.Line, position.Column, cs[0],
                    cs[1]);
                foreach (var clause in cs.Skip(2))
                    disj.AddExpression(clause);
                return disj;
            }
        }

        public void DoEmitPartialApplicationCode(CompilerTarget target)
        {
            if (Conditions.Count == 0)
            {
                this.ConstFunc(!ShortcircuitValue).EmitCode(target);
                return;
            }

            //only the very last condition may be a placeholder
            for (var node = Conditions.First; node != null; node = node.Next)
            {
                var isPlaceholder = node.Value.IsPlaceholder();
                if (node.Next == null)
                {
                    if (!isPlaceholder)
                    {
                        //there is no placeholder at all, wrap expression in const
                        Debug.Assert(Conditions.All(e => !e.IsPlaceholder()));
                        DoEmitCode(target);
                        target.EmitCommandCall(this, 1, Const.Alias);
                        return;
                    }
                }
                else
                {
                    if (isPlaceholder)
                    {
                        _reportInvalidPlaceholders(target);
                        return;
                    }
                }
            }

            //We have expression of the form `e1 and e2 and e3 and ... and ?i`
            var placeholder = (AstPlaceholder) Conditions.Last.Value;
            AstPlaceholder.DeterminePlaceholderIndices(placeholder.Singleton());


            // compile the following code: `if(e1 and e2 and e3) id(?) else const(false)`
            var constExpr = CreatePrefix(this, Conditions.Take(Conditions.Count - 1));
            //var identityFunc = new AstGetSetSymbol(File, Line, Column, PCall.Get, Commands.Core.Id.Alias, SymbolInterpretations.Command);
            //identityFunc.Arguments.Add(new AstPlaceholder(File, Line, Column, placeholder.Index));
            var identityFunc = new AstTypecast(File, Line, Column,
                placeholder.GetCopy(),
                new AstConstantTypeExpression(File, Line, Column, PType.Bool.ToString()));
            var conditional = new AstConditionalExpression(File, Line, Column, constExpr,
                ShortcircuitValue)
                {
                    IfExpression = identityFunc,
                    ElseExpression = this.ConstFunc(ShortcircuitValue)
                };
            conditional.EmitCode(target);
        }

        private void _reportInvalidPlaceholders(CompilerTarget target)
        {
            target.Loader.ReportSemanticError(Line, Column,
                "In partial applications of lazy expressions, only one placeholder at the end of a sequence is allowed. Consider using a lambda expression instead.");
        }

        /// <summary>
        ///     Determines which value (true/false) will be propagated when the prefix evaluates to that value.
        /// </summary>
        protected abstract bool ShortcircuitValue { get; }

        protected virtual IAstExpression CreatePrefix(ISourcePosition position,
            IEnumerable<IAstExpression> clauses)
        {
            throw new NotSupportedException("The lazy logical expression " + GetType().Name +
                " must implement this method/property to support partial application.");
        }

        public virtual bool TryOptimize(CompilerTarget target, out IAstExpression expr)
        {
            expr = null;
            var placeholders = Conditions.Count(AstPartiallyApplicable.IsPlaceholder);
            for (var node = Conditions.First; node != null; node = node.Next)
            {
                var condition = node.Value;
                _OptimizeNode(target, ref condition);
                node.Value = condition; //Update list of conditions with optimized condition

                PValue resultP;
                if (condition is AstConstant &&
                    ((AstConstant) condition).ToPValue(target).TryConvertTo(target.Loader,
                        PType.Bool, false, out resultP))
                {
                    if ((bool) resultP.Value == ShortcircuitValue)
                    {
                        // Expr1 OP shortcircuit OP Expr2 = shortcircuit
                        // Expr1 OP shortcircuit OP Expr2 OP ? = const(shortcircuit)
                        var shortcircuitConst = new AstConstant(condition.File, condition.Line,
                            condition.Column, ShortcircuitValue);
                        if (placeholders > 0)
                        {
                            if (!Conditions.Last.Value.IsPlaceholder() || placeholders > 1)
                                _reportInvalidPlaceholders(target);

                            expr = shortcircuitConst.ConstFunc();
                        }
                        else
                        {
                            expr = shortcircuitConst;
                        }
                        return true;
                    }
                    else
                    {
                        // Expr1 OP ¬shortcircuit OP Expr2 = Expr1 OP Expr2
                        Conditions.Remove(node);
                    }
                }
                else if (condition is AstPlaceholder)
                {
                    placeholders++;
                }
            }

            if (Conditions.Count == 0)
            {
                expr = new AstConstant(File, Line, Column, !ShortcircuitValue);
            }
            else if (Conditions.Count == 1)
            {
                var primaryExpr = Conditions.First.Value;
                expr = _GetOptimizedNode(target,
                    new AstTypecast(primaryExpr.File, primaryExpr.Line, primaryExpr.Column,
                        primaryExpr,
                        new AstConstantTypeExpression(primaryExpr.File,
                            primaryExpr.Line,
                            primaryExpr.Column,
                            PType.Bool.ToString())));
            }

            return expr != null;
        }
    }
}