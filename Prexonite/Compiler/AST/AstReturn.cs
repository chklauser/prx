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
using System.Linq;
using Prexonite.Types;

namespace Prexonite.Compiler.Ast
{
    public class AstReturn : AstNode,
                             IAstHasExpressions
    {
        public ReturnVariant ReturnVariant;
        public AstExpr Expression;

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

        public AstExpr[] Expressions
        {
            get { return new[] {Expression}; }
        }

        #endregion

        protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
        {
            if(stackSemantics == StackSemantics.Value)
                throw new NotSupportedException("Return nodes cannot be used with value stack semantics. (They don't produce any values)");

            var warned = false;
            if (target.Function.Meta[Coroutine.IsCoroutineKey].Switch)
                _warnInCoroutines(target, ref warned);

            if (Expression != null)
            {
                _OptimizeNode(target, ref Expression);
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
                    Expression.EmitValueCode(target);
                    target.Emit(this, OpCode.ret_set);
                    break;
                case ReturnVariant.Continue:
                    if (Expression != null)
                    {
                        Expression.EmitValueCode(target);
                        target.Emit(this, OpCode.ret_set);
                        _warnInCoroutines(target, ref warned);
                    }
                    target.Emit(this, OpCode.ret_continue);
                    break;
                case ReturnVariant.Break:
                    target.Emit(this, OpCode.ret_break);
                    break;
            }
        }

        private void _warnInCoroutines(CompilerTarget target, ref bool warned)
        {
            if (!warned && _isInProtectedBlock(target))
            {
                target.Loader.ReportMessage(new Message(MessageSeverity.Warning,
                    "Detected possible return (yield) from within a protected block " +
                        "(try-catch-finally, using, foreach). " +
                            "This Prexonite implementation cannot guarantee that cleanup code is executed. ",
                    this));
                warned = true;
            }
        }

        private static bool _isInProtectedBlock(CompilerTarget target)
        {
            return
                target.ScopeBlocks.OfType<AstScopedBlock>().Any(
                    sb => (sb.LexicalScope is AstForeachLoop) ||
                        (sb.LexicalScope is AstTryCatchFinally) || (sb.LexicalScope is AstUsing));
        }

        private void emit_tail_call_exit(CompilerTarget target)
        {
            if (optimize_conditional_return_expression(target))
                return;

            var getset = Expression as AstGetSet;
            var symbol = Expression as AstGetSetSymbol;
            var icbr = Expression as ICanBeReferenced;

            AstExpr reference;
            if ((getset != null && getset.Call == PCall.Set ||
                //the 'value' of set-expressions is not the return value of the call
                (symbol != null && symbol.IsObjectVariable)) ||
                    icbr == null || !icbr.TryToReference(out reference))
                //tail requires a reference to the continuation
            {
                //Cannot be tail call optimized
                Expression.EmitValueCode(target);
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
                            symbolArgs[i].EmitValueCode(target);
                        else
                            nullNode.EmitValueCode(target);
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
                    Expression.EmitValueCode(target);
                    target.Emit(this, OpCode.ret_value);
                    return;
                }
            }
        }

        private static bool _isStacklessRecursionPossible(CompilerTarget target,
            AstGetSetSymbol symbol)
        {
            if (symbol.Implementation.Interpretation != SymbolInterpretations.Function) //must be function call
                return false;
            if(symbol.Implementation.Module != target.Loader.ParentApplication.Module.Name) //must be direct recursive iteration
                return false;
            if (!Engine.StringsAreEqual(target.Function.Id, symbol.Implementation.InternalId))
                //must be direct recursive iteration
                return false;
            if (target.Function.Variables.Contains(PFunction.ArgumentListId))
                //must not use argument list
                return false;
            if (symbol.Arguments.Count > target.Function.Parameters.Count)
                //must not supply more arguments than mapped
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
            var retif = new AstCondition(this, target.CurrentBlock, cond.Condition, cond.IsNegative);

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

            retif.EmitEffectCode(target); //  if( cond )
            //      return expr1
            ret2.EmitEffectCode(target); //  return expr2

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