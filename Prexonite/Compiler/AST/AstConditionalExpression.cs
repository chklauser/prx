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
using Prexonite.Types;

namespace Prexonite.Compiler.Ast;

public class AstConditionalExpression : AstExpr,
    IAstHasExpressions
{
    public AstConditionalExpression(
        string file, int line, int column, AstExpr condition, bool isNegative)
        : base(file, line, column)
    {
        Condition = condition ?? throw new ArgumentNullException(nameof(condition));
        IsNegative = isNegative;
    }

    public AstConditionalExpression(string file, int line, int column, AstExpr condition)
        : this(file, line, column, condition, false)
    {
    }

    internal AstConditionalExpression(Parser p, AstExpr condition, bool isNegative)
        : this(p.scanner.File, p.t.line, p.t.col, condition, isNegative)
    {
    }

    internal AstConditionalExpression(Parser p, AstExpr condition)
        : this(p, condition, false)
    {
    }

    public AstExpr IfExpression;
    public AstExpr ElseExpression;
    public AstExpr Condition;
    public bool IsNegative;
    private static int _depth;

    #region IAstHasExpressions Members

    public AstExpr[] Expressions
    {
        get { return new[] {Condition, IfExpression, ElseExpression}; }
    }

    #endregion

    #region AstExpr Members

    public override bool TryOptimize(CompilerTarget target, out AstExpr expr)
    {
        //Optimize condition
        _OptimizeNode(target, ref Condition);
        // Invert condition when unary logical not
        while (Condition.IsCommandCall(Commands.Core.Operators.LogicalNot.DefaultAlias, out var unaryCond))
        {
            Condition = unaryCond.Arguments[0];
            IsNegative = !IsNegative;
        }

        //Constant conditions
        if (Condition is AstConstant constCond)
        {
            if (!constCond.ToPValue(target).TryConvertTo(target.Loader, PType.Bool, out var condValue))
                expr = null;
            else if ((bool) condValue.Value ^ IsNegative)
                expr = IfExpression;
            else
                expr = ElseExpression;
            return expr != null;
        }

        expr = null;
        return false;
    }

    #endregion

    protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
    {
        //Optimize condition
        _OptimizeNode(target, ref Condition);
        _OptimizeNode(target, ref IfExpression);
        _OptimizeNode(target, ref ElseExpression);

        var elseLabel = "elsei\\" + _depth + "\\assembler";
        var endLabel = "endifi\\" + _depth + "\\assembler";
        _depth++;

        //Emit
        //if => block / else => block
        AstLazyLogical.EmitJumpCondition(target, Condition, elseLabel, IsNegative);
        IfExpression.EmitCode(target, stackSemantics);
        target.EmitJump(Position, endLabel);
        target.EmitLabel(Position, elseLabel);
        ElseExpression.EmitCode(target, stackSemantics);
        target.EmitLabel(Position, endLabel);

        target.FreeLabel(elseLabel);
        target.FreeLabel(endLabel);
    }
}