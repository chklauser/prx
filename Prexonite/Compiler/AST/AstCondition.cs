

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
        get { return [IfBlock, ElseBlock]; }
    }

    #region IAstHasExpressions Members

    public AstExpr[] Expressions
    {
        get { return [Condition]; }
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