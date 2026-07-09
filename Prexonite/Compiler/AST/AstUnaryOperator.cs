

using System.Diagnostics;
using JetBrains.Annotations;
using Prexonite.Modular;
using Prexonite.Properties;

namespace Prexonite.Compiler.Ast;

public class AstUnaryOperator : AstExpr,
    IAstHasExpressions,
    IAstPartiallyApplicable
{
    public AstUnaryOperator(ISourcePosition position, UnaryOperator op, AstExpr operand)
        : base(position)
    {
        Operator = op;
        Operand = operand ?? throw new ArgumentNullException(nameof(operand));
    }

    #region IAstHasExpressions Members

    public AstExpr[] Expressions => [Operand];

    public UnaryOperator Operator { get; }

    public AstExpr Operand { get; }

    #endregion

    #region AstExpr Members

    public override bool TryOptimize(CompilerTarget target, [NotNullWhen(true)] out AstExpr? expr)
    {
        expr = null;
        var operand = Operand;
        _OptimizeNode(target, ref operand);
        if (operand is AstConstant constOperand) 
        {
            var valueOperand = constOperand.ToPValue(target);
            PValue? result;
            switch (Operator)
            {
                case UnaryOperator.UnaryNegation:
                    if (valueOperand.UnaryNegation(target.Loader, out result))
                        goto emitConstant;
                    break;
                case UnaryOperator.LogicalNot:
                    if (valueOperand.LogicalNot(target.Loader, out result))
                        goto emitConstant;
                    break;
                case UnaryOperator.OnesComplement:
                    if (valueOperand.OnesComplement(target.Loader, out result))
                        goto emitConstant;
                    break;
                case UnaryOperator.PreIncrement:
                    if (valueOperand.Increment(target.Loader, out result))
                        goto emitConstant;
                    break;
                case UnaryOperator.PreDecrement:
                    if (valueOperand.Decrement(target.Loader, out result))
                        goto emitConstant;
                    break;
                case UnaryOperator.PostIncrement:
                case UnaryOperator.PostDecrement:
                case UnaryOperator.None:
                case UnaryOperator.PreDeltaLeft:
                case UnaryOperator.PostDeltaLeft:
                case UnaryOperator.PreDeltaRight:
                case UnaryOperator.PostDeltaRight:
                    //No optimization allowed/needed here
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(Operator), Operator, "Unknown unary operator.");
            }
            goto emitFull;

            emitConstant:
            return AstConstant.TryCreateConstant(target, Position, result, out expr);
            emitFull:
            return false;
        }

        //Try other optimizations
        switch (Operator)
        {
            case UnaryOperator.UnaryNegation:
            case UnaryOperator.LogicalNot:
            case UnaryOperator.OnesComplement:
                if (operand is AstUnaryOperator doubleNegation && doubleNegation.Operator == Operator)
                {
                    expr = doubleNegation.Operand;
                    return true;
                }
                break;
            case UnaryOperator.PreIncrement:
            case UnaryOperator.PreDecrement:
            case UnaryOperator.PostIncrement:
            case UnaryOperator.PostDecrement:
            case UnaryOperator.None:
            case UnaryOperator.PreDeltaLeft:
            case UnaryOperator.PostDeltaLeft:
            case UnaryOperator.PreDeltaRight:
            case UnaryOperator.PostDeltaRight:
                //No optimization
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(Operator), Operator, "Unknown unary operator.");
        }
        return false;
    }

    #endregion

    void _emitIncrementDecrementCode(CompilerTarget target, StackSemantics value)
    {
        if (Operand.IsArgumentSplice())
        {
            AstArgumentSplice.ReportNotSupported(Operand, target, value);
            return;
        }
            
        var symbolCall = Operand as AstIndirectCall;
        var symbol = symbolCall?.Subject as AstReference;
        EntityRef.Variable? variableRef = null;
        var isVariable = symbol != null && symbol.Entity.TryGetVariable(out variableRef);
        var isPre = Operator == UnaryOperator.PreDecrement || Operator == UnaryOperator.PreIncrement;
        switch (Operator)
        {
            case UnaryOperator.PreIncrement:
            case UnaryOperator.PostIncrement:
            case UnaryOperator.PreDecrement:
            case UnaryOperator.PostDecrement:
                var isIncrement = 
                    Operator == UnaryOperator.PostIncrement ||
                    Operator == UnaryOperator.PreIncrement;
                if (isVariable)
                {
                    Debug.Assert(variableRef != null);
                    Action loadVar;
                    Action perform;

                    // First setup the two actions
                    if (variableRef.TryGetLocalVariable(out var localRef))
                    {
                        loadVar = () => target.EmitLoadLocal(Position, localRef.Id);
                        perform = () => 
                            target.Emit(Position, isIncrement ? OpCode.incloc : OpCode.decloc, localRef.Id);
                    }
                    else if(variableRef.TryGetGlobalVariable(out var globalRef))
                    {
                        loadVar = () => target.EmitLoadGlobal(Position, globalRef.Id, globalRef.ModuleName);

                        perform = () =>
                            target.Emit(Position, isIncrement ? OpCode.incglob : OpCode.decglob, globalRef.Id,
                                globalRef.ModuleName);
                    }
                    else
                    {
                        throw new InvalidOperationException("Found variable entity that is neither a global nor a local variable.");
                    }

                    // Then decide in what order to apply them.
                    if (!isPre && value == StackSemantics.Value)
                    {
                        loadVar();
                    }

                    perform();

                    if (isPre && value == StackSemantics.Value)
                    {
                        loadVar();
                    }
                }
                else
                    throw new PrexoniteException(
                        "Node of type " + Operand.GetType() +
                        " does not support increment/decrement operators.");
                break;
            // ReSharper disable RedundantCaseLabel
            case UnaryOperator.UnaryNegation:
            case UnaryOperator.LogicalNot:
            case UnaryOperator.OnesComplement:
            // ReSharper restore RedundantCaseLabel
            // ReSharper disable RedundantEmptyDefaultSwitchBranch
            default:
                break; //No effect
            // ReSharper restore RedundantEmptyDefaultSwitchBranch
        }
    }

    protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
    {
        if (target == null)
            throw new ArgumentNullException(nameof(target));

        switch (stackSemantics)
        {
            case StackSemantics.Value:
                _emitValueCode(target);
                break;
            case StackSemantics.Effect:
                _emitIncrementDecrementCode(target, stackSemantics);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(stackSemantics));
        }
    }

    void _emitValueCode(CompilerTarget target)
    {
        switch (Operator)
        {
            case UnaryOperator.LogicalNot:
            case UnaryOperator.UnaryNegation:
            case UnaryOperator.OnesComplement:
                target.Loader.ReportMessage(
                    Message.Error(
                        Resources.AstUnaryOperator__NonIncrementDecrement,
                        Position, MessageClasses.ParserInternal));
                break;
            case UnaryOperator.PreDecrement:
            case UnaryOperator.PreIncrement:
                _emitIncrementDecrementCode(target, StackSemantics.Value);
                break;
            case UnaryOperator.PostDecrement:
            case UnaryOperator.PostIncrement:
                _emitIncrementDecrementCode(target, StackSemantics.Value);
                break;
        }
    }

    public NodeApplicationState CheckNodeApplicationState()
    {
        return new(
            Operand.IsPlaceholder()
            || Operator == UnaryOperator.LogicalNot
            && Operand is AstTypecheck typecheck
            && typecheck.CheckForPlaceholders(),
            Operand.IsArgumentSplice());
    }

    public void DoEmitPartialApplicationCode(CompilerTarget target)
    {
        //Just emit the operator normally, the appropriate mechanism will kick in
        // further down the AST
        DoEmitCode(target,StackSemantics.Value);
    }
}

public enum UnaryOperator
{
    [PublicAPI]
    None,
    UnaryNegation,
    LogicalNot,
    OnesComplement,
    PreIncrement,
    PreDecrement,
    PostIncrement,
    PostDecrement,
    PreDeltaLeft,
    PostDeltaLeft,
    PreDeltaRight,
    PostDeltaRight,
}