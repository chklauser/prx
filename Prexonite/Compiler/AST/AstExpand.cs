using Prexonite.Compiler.Macro;
using Prexonite.Modular;

namespace Prexonite.Compiler.Ast;

public class AstExpand : AstGetSetImplBase, IAstPartiallyApplicable
{
    public AstExpand(ISourcePosition position, EntityRef entity, PCall call)
        : base(position, call)
    {
        Entity = entity;
    }

    public EntityRef Entity { get; }

    protected override void EmitGetCode(CompilerTarget target, StackSemantics stackSemantics)
    {
        throw new NotSupportedException(
            "Macro expansion requires a different mechanism. Use AstGetSet.EmitCode instead."
        );
    }

    protected override void EmitSetCode(CompilerTarget target)
    {
        throw new NotSupportedException(
            "Macro expansion requires a different mechanism. Use AstGetSet.EmitCode instead."
        );
    }

    protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
    {
        //instantiate macro for the current target
        MacroSession? session = null;

        try
        {
            //Acquire current macro session
            session = target.AcquireMacroSession();

            //Expand macro
            var justEffect = stackSemantics == StackSemantics.Effect;
            var node = session.ExpandMacro(this, justEffect);

            //Emit generated code
            node.EmitCode(target, stackSemantics);
        }
        finally
        {
            if (session != null)
                target.ReleaseMacroSession(session);
        }
    }

    void IAstPartiallyApplicable.DoEmitPartialApplicationCode(CompilerTarget target)
    {
        // This may fail if the macro implementation does not support partial application.
        DoEmitCode(target, StackSemantics.Value);
    }

    public override bool TryOptimize(CompilerTarget target, [NotNullWhen(true)] out AstExpr? expr)
    {
        //Do not optimize the macros arguments! They should be passed to the macro in their original form.
        //  the macro should decide whether or not to apply AST-optimization to the arguments or not.
        expr = null;
        return false;
    }

    public override AstGetSet GetCopy()
    {
        var copy = new AstExpand(Position, Entity, Call);
        CopyBaseMembers(copy);
        return copy;
    }

    public override string ToString()
    {
        return $"expand {(Enum.GetName(typeof(PCall), Call) ?? "-").ToLowerInvariant()}: {Entity}({ArgumentsToString()})";
    }
}
