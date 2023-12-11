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

using Prexonite.Commands.Core.Operators;

namespace Prexonite.Compiler.Ast;

public class AstCondition : AstNode,
    IAstHasBlocks,
    IAstHasExpressions
{
    public AstCondition(ISourcePosition p, AstBlock parentBlock, AstExpr condition, bool isNegative = false)
        : base(p)
    {
        IfBlock = new(p,parentBlock,prefix: "if");
        ElseBlock = new(p,parentBlock,prefix:"else");
        Condition = condition ?? throw new ArgumentNullException(nameof(condition));
        IsNegative = isNegative;
    }

    public AstScopedBlock IfBlock;
    public AstScopedBlock ElseBlock;
    public AstExpr Condition;
    public bool IsNegative;
    static int _depth;

    #region IAstHasBlocks Members

    public AstBlock[] Blocks
    {
        get { return new AstBlock[] {IfBlock, ElseBlock}; }
    }

    #region IAstHasExpressions Members

    public AstExpr[] Expressions
    {
        get { return new[] {Condition}; }
    }

    #endregion

    #endregion

    protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
    {
        //Optimize condition
        _OptimizeNode(target, ref Condition);

        // Invert condition when unary logical not
        while (Condition.IsCommandCall(LogicalNot.DefaultAlias, out var unaryCond))
        {
            Condition = unaryCond.Arguments[0];
            IsNegative = !IsNegative;
        }

        //Constant conditions
        if (Condition is AstConstant constCond)
        {
            if (!constCond.ToPValue(target).TryConvertTo(target.Loader, PType.Bool, out var condValue))
                goto continueFull;
            else if ((bool) condValue.Value! ^ IsNegative)
                IfBlock.EmitEffectCode(target);
            else
                ElseBlock.EmitEffectCode(target);
            return;
        }
        //Conditions with empty blocks
        if (IfBlock.IsEmpty && ElseBlock.IsEmpty)
        {
            Condition.EmitEffectCode(target);
            return;
        }
        continueFull:
        ;

        //Switch If and Else block in case the if-block is empty
        if (IfBlock.IsEmpty)
        {
            IsNegative = !IsNegative;
            var tmp = IfBlock;
            IfBlock = ElseBlock;
            ElseBlock = tmp;
        }

        var elseLabel = "else\\" + _depth + "\\assembler";
        var endLabel = "endif\\" + _depth + "\\assembler";
        _depth++;

        //Emit
        var ifGoto = IfBlock.IsSingleStatement
            ? IfBlock[0] as AstExplicitGoTo
            : null;
        var elseGoto = ElseBlock.IsSingleStatement
            ? ElseBlock[0] as AstExplicitGoTo
            : null;

        if (ifGoto != null && elseGoto != null)
        {
            //only jumps
            AstLazyLogical.EmitJumpCondition(
                target,
                Condition,
                ifGoto.Destination,
                elseGoto.Destination,
                !IsNegative);
        }
        else if (ifGoto != null)
        {
            //if => jump / else => block
            AstLazyLogical.EmitJumpCondition(target, Condition, ifGoto.Destination, !IsNegative);
            ElseBlock.EmitEffectCode(target);
        }
        else if (elseGoto != null)
        {
            //if => block / else => jump
            AstLazyLogical.EmitJumpCondition(
                target, Condition, elseGoto.Destination, IsNegative); //inverted
            IfBlock.EmitEffectCode(target);
        }
        else
        {
            //if => block / else => block
            AstLazyLogical.EmitJumpCondition(target, Condition, elseLabel, IsNegative);
            IfBlock.EmitEffectCode(target);
            target.EmitJump(Position, endLabel);
            target.EmitLabel(Position, elseLabel);
            ElseBlock.EmitEffectCode(target);
            target.EmitLabel(Position, endLabel);
        }

        target.FreeLabel(elseLabel);
        target.FreeLabel(endLabel);
    }
}