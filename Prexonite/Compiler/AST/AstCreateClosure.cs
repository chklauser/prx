using System.Diagnostics;
using Prexonite.Modular;

namespace Prexonite.Compiler.Ast;

[DebuggerNonUserCode]
public class AstCreateClosure : AstExpr
{
    public AstCreateClosure(ISourcePosition position, EntityRef.Function implementation)
        : base(position)
    {
        Implementation = implementation;
    }

    public EntityRef.Function Implementation { get; }

    protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
    {
        if (stackSemantics == StackSemantics.Effect)
            return;

        if (
            target.Loader.ParentApplication.TryGetFunction(
                Implementation.Id,
                Implementation.ModuleName,
                out var targetFunction
            )
            && (
                !targetFunction.Meta.TryGetValue(PFunction.SharedNamesKey, out var sharedNamesEntry)
                || !sharedNamesEntry.IsList
                || sharedNamesEntry.List.Length == 0
            )
        )
            target.Emit(
                Position,
                OpCode.ldr_func,
                Implementation.Id,
                target.ToInternalModule(Implementation.ModuleName)
            );
        else
            target.Emit(
                Position,
                OpCode.newclo,
                Implementation.Id,
                target.ToInternalModule(Implementation.ModuleName)
            );
    }

    #region AstExpr Members

    public override bool TryOptimize(CompilerTarget target, [NotNullWhen(true)] out AstExpr? expr)
    {
        expr = null;
        return false;
    }

    #endregion
}
