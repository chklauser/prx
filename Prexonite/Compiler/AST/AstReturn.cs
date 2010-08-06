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
using System.Collections.Generic;
using Prexonite.Types;

namespace Prexonite.Compiler.Ast
{
    public class AstReturn : AstNode,
                             IAstHasExpressions
    {
        public ReturnVariant ReturnVariant;
        public IAstExpression Expression;

        public AstReturn(string file, int line, int column, ReturnVariant returnVariant)
            : base(file, line, column)
        {
            ReturnVariant = returnVariant;
        }

        internal AstReturn(Parser p, ReturnVariant returnVariant)
            : this(p.scanner.File, p.t.line, p.t.col, returnVariant)
        {
        }

        #region IAstHasExpressions Members

        public IAstExpression[] Expressions
        {
            get { return new[] {Expression}; }
        }

        #endregion

        public override void EmitCode(CompilerTarget target)
        {
            if (Expression != null)
            {
                OptimizeNode(target, ref Expression);
                if (ReturnVariant == ReturnVariant.Exit)
                {
                    emit_tail_call_exit(target);
                    return;
                }
            }
            switch (ReturnVariant)
            {
                case ReturnVariant.Exit:
                    target.Emit(this, OpCode.ret_exit);
                    break;
                case ReturnVariant.Set:
                    if (Expression == null)
                        throw new PrexoniteException("Return assignment requires an expression.");
                    Expression.EmitCode(target);
                    target.Emit(this, OpCode.ret_set);
                    break;
                case ReturnVariant.Continue:
                    if (Expression != null)
                    {
                        Expression.EmitCode(target);
                        target.Emit(this, OpCode.ret_set);
                    }
                    target.Emit(this, OpCode.ret_continue);
                    break;
                case ReturnVariant.Break:
                    target.Emit(this, OpCode.ret_break);
                    break;
            }
        }

        private void emit_tail_call_exit(CompilerTarget target)
        {
            if (optimize_conditional_return_expression(target))
                return;

            var getset = Expression as AstGetSet;
            var symbol = Expression as AstGetSetSymbol;
            var icbr = Expression as ICanBeReferenced;

            AstGetSet reference;
            if ((getset != null && getset.Call == PCall.Set || //the 'value' of set-expressions is not the return value of the call
                 (symbol != null && symbol.IsObjectVariable)) ||
                icbr == null || !icbr.TryToReference(out reference)) //tail requires a reference to the continuation
            {
                //Cannot be tail call optimized
                Expression.EmitCode(target);
                target.Emit(this, OpCode.ret_value);
            }
            else //Will be tail called
            {
                if (symbol != null && _isStacklessRecursionPossible(target, symbol))
                {
                    // specialized approach
                    // self(arg1, arg2, ..., argn) => { param1 = arg1; param2 = arg2; ... paramn = argn; goto 0; }
                    var symbolParams = target.Function.Parameters;
                    var symbolArgs = symbol.Arguments;
                    var nullNode = new AstNull(File, Line, Column);

                    //copy parameters to temporary variables
                    for (var i = 0; i < symbolParams.Count; i++)
                    {
                        if (i < symbolArgs.Count)
                            symbolArgs[i].EmitCode(target);
                        else
                            nullNode.EmitCode(target);
                    }
                    //overwrite parameters
                    for (var i = symbolParams.Count - 1; i >= 0; i--)
                    {
                        target.EmitStoreLocal(this, symbolParams[i]);
                    }

                    target.EmitJump(this, 0);
                }
                else
                {
                    Expression.EmitCode(target);
                    target.Emit(this, OpCode.ret_value);
                    return;
                }
            }
        }

        private static bool _isStacklessRecursionPossible(CompilerTarget target, AstGetSetSymbol symbol)
        {
            if (symbol.Interpretation != SymbolInterpretations.Function) //must be function call
                return false;
            if (!Engine.StringsAreEqual(target.Function.Id, symbol.Id)) //must be direct recursive iteration
                return false;
            if (target.Function.Variables.Contains(PFunction.ArgumentListId)) //must not use argument list
                return false;
            if (symbol.Arguments.Count > target.Function.Parameters.Count) //must not supply more arguments than mapped
                return false;
            return true;
        }

        private bool optimize_conditional_return_expression(CompilerTarget target)
        {
            var cond = Expression as AstConditionalExpression;
            if (cond == null)
                return false;

            //  return  if( cond )
            //              expr1
            //          else
            //              expr2
            var retif = new AstCondition(File, Line, Column, cond.Condition);

            var ret1 = new AstReturn(File, Line, Column, ReturnVariant)
            {
                Expression = cond.IfExpression
            };
            retif.IfBlock.Add(ret1);

            var ret2 = new AstReturn(File, Line, Column, ReturnVariant)
            {
                Expression = cond.ElseExpression
            };
            //not added to the condition

            retif.EmitCode(target); //  if( cond )
            //      return expr1
            ret2.EmitCode(target); //  return expr2

            //ret1 and ret2 will continue optimizing
            return true;
        }

        public override string ToString()
        {
            var format = "";
            switch (ReturnVariant)
            {
                case ReturnVariant.Exit:
                    format = Expression != null ? "return {0};" : "return;";
                    break;
                case ReturnVariant.Set:
                    format = "return = {0};";
                    break;
                case ReturnVariant.Continue:
                    format = Expression != null ? "yield {0};" : "continue;";
                    break;
                case ReturnVariant.Break:
                    format = "break;";
                    break;
            }
            return String.Format(format, Expression);
        }
    }

    public enum ReturnVariant
    {
        Exit,
        Break,
        Continue,
        Set
    }
}