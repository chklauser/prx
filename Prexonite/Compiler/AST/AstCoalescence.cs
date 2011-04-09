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
using System.Linq;
using Prexonite.Commands.Core;
using Prexonite.Types;

namespace Prexonite.Compiler.Ast
{
    public class AstCoalescence : AstNode,
                                  IAstExpression,
                                  IAstHasExpressions,
                                    IAstPartiallyApplicable
    {
        public AstCoalescence(string file, int line, int column)
            : base(file, line, column)
        {
        }

        internal AstCoalescence(Parser p)
            : base(p)
        {
        }

        private readonly List<IAstExpression> _expressions = new List<IAstExpression>(2);

        #region IAstHasExpressions Members

        IAstExpression[] IAstHasExpressions.Expressions
        {
            get { return _expressions.ToArray(); }
        }

        public List<IAstExpression> Expressions
        {
            get { return _expressions; }
        }

        #endregion

        #region IAstExpression Members

        public bool TryOptimize(CompilerTarget target, out IAstExpression expr)
        {
            expr = null;

            //Optimize arguments
            for (var i = 0; i < _expressions.Count; i++)
            {
                var arg = _expressions[i];
                if (arg == null)
                    throw new PrexoniteException(
                        "Invalid (null) argument in GetSet node (" + ToString() +
                        ") detected at position " + _expressions.IndexOf(arg) + ".");
                var oArg = _GetOptimizedNode(target, arg);
                if (!ReferenceEquals(oArg, arg))
                    _expressions[i] = oArg;
            }

            var nonNullExpressions = _expressions.Where(_exprIsNotNull).ToArray();
            _expressions.Clear();
            _expressions.AddRange(nonNullExpressions);

            if (_expressions.Count == 1)
            {
                var pExpr = _expressions[0];
                expr = pExpr is AstPlaceholder ? ((AstPlaceholder)pExpr).IdFunc() : pExpr;
                return true;
            }
            else if (_expressions.Count == 0)
            {
                expr = new AstNull(File, Line, Column);
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool _exprIsNotNull(IAstExpression iexpr)
        {
            return !(iexpr is AstNull ||
                   (iexpr is AstConstant && ((AstConstant) iexpr).Constant == null));
        }

        #endregion

        private static int _count = -1;
        private static readonly object _labelCountLock = new object();

        protected override void DoEmitCode(CompilerTarget target)
        {
            //Expressions contains at least two expressions
            var endLabel = _generateEndLabel();
            _emitCode(target, endLabel);
            target.EmitLabel(this, endLabel);
        }

        private void _emitCode(CompilerTarget target, string endLabel)
        {
            for (var i = 0; i < _expressions.Count; i++)
            {
                var expr = _expressions[i];

                if (i > 0)
                    target.EmitPop(this);

                expr.EmitCode(target);

                if (i + 1 >= _expressions.Count)
                    continue;
                target.EmitDuplicate(this);
                target.Emit(this, OpCode.check_null);
                target.EmitJumpIfFalse(this, endLabel);
            }

           
        }

        private static string _generateEndLabel()
        {
            lock (_labelCountLock)
            {
                _count++;
                return "coal\\n" + _count + "\\end";
            }
        }

        public override bool CheckForPlaceholders()
        {
            return base.CheckForPlaceholders() || Expressions.Any(AstPartiallyApplicable.IsPlaceholder);
        }

        public void DoEmitPartialApplicationCode(CompilerTarget target)
        {
            AstPlaceholder.DeterminePlaceholderIndices(Expressions.OfType<AstPlaceholder>());

            var count = Expressions.Count;
            if (count == 0)
            {
                this.ConstFunc(null).EmitCode(target);
                return;
            }

            //only the very last condition may be a placeholder
            for (var i = 0; i < count; i++)
            {
                var value = Expressions[i];
                var isPlaceholder = value.IsPlaceholder();
                if (i == count - 1)
                {
                    if (!isPlaceholder)
                    {
                        //there is no placeholder at all, wrap expression in const
                        System.Diagnostics.Debug.Assert(Expressions.All(e => !e.IsPlaceholder()));
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

            if(count == 0)
            {
                this.ConstFunc(null).EmitCode(target);
            }
            else if(count == 1)
            {
                System.Diagnostics.Debug.Assert(Expressions[0].IsPlaceholder(), "Singleton ??-chain expected to consist of placeholder.");
                var placeholder = (AstPlaceholder)Expressions[0];
                placeholder.IdFunc().EmitCode(target);
            }
            else
            {
                System.Diagnostics.Debug.Assert(Expressions[count-1].IsPlaceholder(), "Last expression in ??-chain expected to be placeholder.");
                var placeholder = (AstPlaceholder) Expressions[count - 1];
                var prefix = new AstCoalescence(File, Line, Column);
                prefix.Expressions.AddRange(Expressions.Take(count-1));

                //check for null (keep a copy of prefix on stack)
                var constLabel = _generateEndLabel();
                var endLabel = _generateEndLabel();
                prefix._emitCode(target, constLabel);
                target.EmitDuplicate(this);
                target.Emit(this, OpCode.check_null);
                target.EmitJumpIfFalse(this, constLabel);
                //prefix is null, identity function
                target.EmitPop(this); 
                placeholder.IdFunc().EmitCode(target);
                target.EmitJump(this, endLabel);
                //prefix is not null, const function
                target.EmitLabel(this, constLabel);
                target.EmitCommandCall(this, 1, Const.Alias);
                target.EmitLabel(this, endLabel);
            }
        }

        private void _reportInvalidPlaceholders(CompilerTarget target)
        {
            target.Loader.ReportSemanticError(Line, Column, "In partial applications of lazy coalescence expressions, only one placeholder at the end of a sequence is allowed. Consider using a lambda expression instead.");
        }
    }
}