

using JetBrains.Annotations;
using Prexonite.Modular;

namespace Prexonite.Compiler.Ast;

public class AstKeyValuePair : AstExpr,
    IAstHasExpressions, IAstPartiallyApplicable
{
    AstExpr _key;
    AstExpr _value;

    [PublicAPI]
    public AstKeyValuePair(
        string file, int line, int column, AstExpr key, AstExpr value)
        : base(file, line, column)
    {
        _key = key;
        _value = value;
    }

    public AstExpr Key => _key;

    public AstExpr Value => _value;

    #region IAstHasExpressions Members

    public AstExpr[] Expressions => [Key, Value];

    #endregion

    protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
    {
        if (Key == null)
            throw new PrexoniteException("AstKeyValuePair.Key must be initialized.");
        if (Value == null)
            throw new ArgumentNullException(nameof(target));

        if (Key.IsArgumentSplice())
        {
            AstArgumentSplice.ReportNotSupported(Key, target, stackSemantics);
        }

        if (Value.IsArgumentSplice())
        {
            AstArgumentSplice.ReportNotSupported(Value, target, stackSemantics);
        }

        var call = target.Factory.Call(Position, EntityRef.Command.Create(Engine.PairAlias));
        call.Arguments.Add(Key);
        call.Arguments.Add(Value);
        call.EmitCode(target, stackSemantics);
    }

    #region AstExpr Members

    public override bool TryOptimize(CompilerTarget target, [NotNullWhen(true)] out AstExpr? expr)
    {
        if (Key == null)
            throw new PrexoniteException("AstKeyValuePair.Key must be initialized.");
        if (Value == null)
            throw new ArgumentNullException(nameof(target));

        _OptimizeNode(target, ref _key);
        _OptimizeNode(target, ref _value);

        expr = null;

        return false;
    }

    #endregion

    #region Implementation of IAstPartiallyApplicable

    public NodeApplicationState CheckNodeApplicationState()
    {
        return new(
            Key.IsPlaceholder() || Value.IsPlaceholder(), 
            Key.IsArgumentSplice() || Value.IsArgumentSplice());
    }

    public void DoEmitPartialApplicationCode(CompilerTarget target)
    {            
        if (Key.IsArgumentSplice())
        {
            AstArgumentSplice.ReportNotSupported(Key, target, StackSemantics.Value);
        }

        if (Value.IsArgumentSplice())
        {
            AstArgumentSplice.ReportNotSupported(Value, target, StackSemantics.Value);
        }
        DoEmitCode(target,StackSemantics.Value);
        //Partial application is handled by AstGetSetSymbol. Code is the same
    }

    #endregion

    public override string ToString()
    {
        var key = Key.ToString() ?? "-null-";
        var value = Value.ToString() ?? "-null-";
        return $"Key = ({key}): Value = ({value})";
    }
}