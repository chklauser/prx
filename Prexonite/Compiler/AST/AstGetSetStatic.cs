using System.Diagnostics;

namespace Prexonite.Compiler.Ast;

public class AstGetSetStatic : AstGetSetImplBase, IAstPartiallyApplicable
{
    public AstTypeExpr TypeExpr { get; private set; }

    [DebuggerStepThrough]
    public AstGetSetStatic(
        ISourcePosition position,
        PCall call,
        AstTypeExpr typeExpr,
        string memberId
    )
        : base(position, call)
    {
        TypeExpr = typeExpr ?? throw new ArgumentNullException(nameof(typeExpr));
        MemberId = memberId ?? throw new ArgumentNullException(nameof(memberId));
    }

    [DebuggerStepThrough]
    internal AstGetSetStatic(Parser p, PCall call, AstTypeExpr typeExpr, string memberId)
        : this(p.GetPosition(), call, typeExpr, memberId) { }

    public string MemberId { get; }

    public override bool TryOptimize(CompilerTarget target, [NotNullWhen(true)] out AstExpr? expr)
    {
        TypeExpr = (AstTypeExpr)_GetOptimizedNode(target, TypeExpr);
        return base.TryOptimize(target, out expr);
    }

    protected override void EmitGetCode(CompilerTarget target, StackSemantics stackSemantics)
    {
        var constType = TypeExpr as AstConstantTypeExpression;

        var justEffect = stackSemantics == StackSemantics.Effect;
        if (constType != null)
        {
            EmitArguments(target);
            target.EmitStaticGetCall(
                Position,
                Arguments.Count,
                constType.TypeExpression,
                MemberId,
                justEffect
            );
        }
        else
        {
            TypeExpr.EmitValueCode(target);
            target.EmitConstant(Position, MemberId);
            EmitArguments(target);
            target.EmitGetCall(
                Position,
                Arguments.Count + 1,
                PType.StaticCallFromStackId,
                justEffect
            );
        }
    }

    /// <summary>
    /// Warning: cannot handle set-expressions, use <see cref="EmitSetCode(Prexonite.Compiler.CompilerTarget,Prexonite.Compiler.Ast.StackSemantics)"/> instead.
    /// </summary>
    /// <param name="target"></param>
    protected override void EmitSetCode(CompilerTarget target)
    {
        EmitSetCode(target, StackSemantics.Effect);
    }

    protected virtual void EmitSetCode(CompilerTarget target, StackSemantics stackSemantics)
    {
        var constType = TypeExpr as AstConstantTypeExpression;
        var justEffect = stackSemantics == StackSemantics.Effect;
        if (constType != null)
        {
            EmitArguments(target, !justEffect, 0);
            target.EmitStaticSetCall(
                Position,
                Arguments.Count,
                constType.TypeExpression + "::" + MemberId
            );
        }
        else
        {
            TypeExpr.EmitValueCode(target);
            target.EmitConstant(Position, MemberId);
            EmitArguments(target, !justEffect, 2);
            //type.StaticCall\FromStack(memberId, args...)
            target.EmitSetCall(Position, Arguments.Count + 1, PType.StaticCallFromStackId);
        }
    }

    protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
    {
        //Do not yet emit arguments.
        if (Call == PCall.Get)
            EmitGetCode(target, stackSemantics);
        else
            EmitSetCode(target, stackSemantics);
    }

    public override AstGetSet GetCopy()
    {
        var copy = new AstGetSetStatic(Position, Call, TypeExpr, MemberId);
        CopyBaseMembers(copy);
        return copy;
    }

    public void DoEmitPartialApplicationCode(CompilerTarget target)
    {
        var argv = AstPartiallyApplicable.PreprocessPartialApplicationArguments(Arguments);
        var ctorArgc = AstPartiallyApplicable.EmitConstructorArguments(this, target, argv);
        if (TypeExpr is AstConstantTypeExpression constTypeExpr)
            target.EmitConstant(constTypeExpr.Position, constTypeExpr.TypeExpression);
        else
            TypeExpr.EmitValueCode(target);
        target.EmitConstant(Position, (int)Call);
        target.EmitConstant(Position, MemberId);
        target.EmitCommandCall(Position, ctorArgc + 3, Engine.PartialStaticCallAlias);
    }

    public override string ToString()
    {
        var name = Enum.GetName(typeof(PCall), Call);
        return $"{name?.ToLowerInvariant() ?? "-"} {TypeExpr}::{MemberId}({Arguments})";
    }
}
