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
using System.Linq;
using Prexonite.Properties;

namespace Prexonite.Compiler.Ast;

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
                _emitTailCallExit(target);
                return;
            }
        }
        switch (ReturnVariant)
        {
            case ReturnVariant.Exit:
                target.Emit(Position,OpCode.ret_exit);
                break;
            case ReturnVariant.Set:
                if (Expression == null)
                    throw new PrexoniteException("Return assignment requires an expression.");
                Expression.EmitValueCode(target);
                target.Emit(Position,OpCode.ret_set);
                break;
            case ReturnVariant.Continue:
                if (Expression != null)
                {
                    Expression.EmitValueCode(target);
                    target.Emit(Position,OpCode.ret_set);
                    _warnInCoroutines(target, ref warned);
                }
                target.Emit(Position,OpCode.ret_continue);
                break;
            case ReturnVariant.Break:
                target.Emit(Position,OpCode.ret_break);
                break;
        }
    }

    void _warnInCoroutines(CompilerTarget target, ref bool warned)
    {
        if (!warned && _isInProtectedBlock(target))
        {
            target.Loader.ReportMessage(Message.Create(MessageSeverity.Warning,
                Resources.AstReturn_Warn_YieldInProtectedBlock,
                Position, MessageClasses.YieldFromProtectedBlock));
            warned = true;
        }
    }

    static bool _isInProtectedBlock(CompilerTarget target)
    {
        return
            target.ScopeBlocks.OfType<AstScopedBlock>().Any(
                sb => sb.LexicalScope is AstForeachLoop ||
                    sb.LexicalScope is AstTryCatchFinally || sb.LexicalScope is AstUsing);
    }

    void _emitTailCallExit(CompilerTarget target)
    {
        if (_optimizeConditionalReturnExpression(target))
            return;

        if (Expression is AstIndirectCall indirectCall
            && _isStacklessRecursionPossible(target, indirectCall))
        {
            // specialized approach
            // self(arg1, arg2, ..., argn) => { param1 = arg1; param2 = arg2; ... paramn = argn; goto 0; }
            _emitRecursiveTailCall(target, indirectCall.Arguments);
        }
        else
        {
            //Cannot be tail call optimized
            _emitOrdinaryValueReturn(target);
        }
    }

    void _emitRecursiveTailCall(CompilerTarget target, ArgumentsProxy symbolArgs)
    {
        var symbolParams = target.Function.Parameters;
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
            target.EmitStoreLocal(Position, symbolParams[i]);
        }

        target.EmitJump(Position, 0);
    }

    void _emitOrdinaryValueReturn(CompilerTarget target)
    {
        Expression.EmitValueCode(target);
        target.Emit(Position, OpCode.ret_value);
    }

    static bool _isStacklessRecursionPossible(CompilerTarget target,
        AstIndirectCall symbol)
    {
        if (!(symbol.Subject is AstReference refNode))
            return false;
        if (!refNode.Entity.TryGetFunction(out var funcRef))
            return false;
        if (funcRef.Id != target.Function.Id)
            return false;
        if (funcRef.ModuleName != target.Function.ParentApplication.Module.Name)
            return false;
        if (target.Function.Variables.Contains(PFunction.ArgumentListId))
            //must not use argument list
            return false;
        if (symbol.Arguments.Count > target.Function.Parameters.Count)
            //must not supply more arguments than mapped
            return false;
        return true;
    }

    bool _optimizeConditionalReturnExpression(CompilerTarget target)
    {
        if (!(Expression is AstConditionalExpression cond))
            return false;

        //  return  if( cond )
        //              expr1
        //          else
        //              expr2
        var retif = new AstCondition(Position, target.CurrentBlock, cond.Condition, cond.IsNegative);

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
        var format = ReturnVariant switch
        {
            ReturnVariant.Exit => Expression != null ? "return {0};" : "return;",
            ReturnVariant.Set => "return = {0};",
            ReturnVariant.Continue => Expression != null ? "yield {0};" : "continue;",
            ReturnVariant.Break => "break;",
            _ => ""
        };
        return string.Format(format, Expression);
    }
}

public enum ReturnVariant
{
    Exit,
    Break,
    Continue,
    Set
}