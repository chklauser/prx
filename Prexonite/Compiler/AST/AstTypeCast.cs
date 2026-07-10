namespace Prexonite.Compiler.Ast;

public class AstTypecast : AstExpr, IAstHasExpressions, IAstPartiallyApplicable
{
    public AstExpr Subject { get; private set; }
    public AstTypeExpr Type { get; private set; }

    public AstTypecast(ISourcePosition position, AstExpr subject, AstTypeExpr type)
        : base(position)
    {
        Subject = subject ?? throw new ArgumentNullException(nameof(subject));
        Type = type ?? throw new ArgumentNullException(nameof(type));
    }

    internal AstTypecast(Parser p, AstExpr subject, AstTypeExpr type)
        : this(p.GetPosition(), subject, type) { }

    #region IAstHasExpressions Members

    public AstExpr[] Expressions
    {
        get { return [Subject]; }
    }

    #endregion

    protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
    {
        if (stackSemantics == StackSemantics.Effect)
            return;

        if (Subject.IsArgumentSplice())
        {
            AstArgumentSplice.ReportNotSupported(Subject, target, stackSemantics);
            return;
        }
        if (Type.IsArgumentSplice())
        {
            AstArgumentSplice.ReportNotSupported(Type, target, stackSemantics);
            return;
        }

        Subject.EmitValueCode(target);
        if (Type is AstConstantTypeExpression constType)
            target.Emit(Position, OpCode.cast_const, constType.TypeExpression);
        else
        {
            Type.EmitValueCode(target);
            target.Emit(Position, OpCode.cast_arg);
        }
    }

    public override bool TryOptimize(CompilerTarget target, [NotNullWhen(true)] out AstExpr? expr)
    {
        Subject = _GetOptimizedNode(target, Subject);
        Type = (AstTypeExpr)_GetOptimizedNode(target, Type);

        expr = null;

        if (Type is not AstConstantTypeExpression constType)
            return false;

        //Constant cast
        if (Subject is AstConstant constSubject)
            return _tryOptimizeConstCast(target, constSubject, constType, out expr);

        //Redundant cast
        AstTypecast? castSubject;
        AstConstantTypeExpression? sndCastType;
        if (
            (castSubject = Subject as AstTypecast) != null
            && (sndCastType = castSubject.Type as AstConstantTypeExpression) != null
        )
        {
            if (Engine.StringsAreEqual(sndCastType.TypeExpression, constType.TypeExpression))
            {
                //remove the outer cast.
                expr = castSubject;
                return true;
            }
        }

        return false;
    }

    bool _tryOptimizeConstCast(
        CompilerTarget target,
        AstConstant constSubject,
        AstConstantTypeExpression constType,
        [NotNullWhen(true)] out AstExpr? expr
    )
    {
        expr = null;
        PType type;
        try
        {
            type = target.Loader.ConstructPType(constType.TypeExpression);
        }
        catch (PrexoniteException)
        {
            //ignore, cast failed. cannot be optimized
            return false;
        }

        if (constSubject.ToPValue(target).TryConvertTo(target.Loader, type, out var result))
            return AstConstant.TryCreateConstant(target, Position, result, out expr);
        else
            return false;
    }

    public NodeApplicationState CheckNodeApplicationState()
    {
        return new(
            Subject.IsPlaceholder() || Type.IsPlaceholder(),
            Subject.IsArgumentSplice() || Type.IsArgumentSplice()
        );
    }

    public void DoEmitPartialApplicationCode(CompilerTarget target)
    {
        if (Subject.IsArgumentSplice())
        {
            AstArgumentSplice.ReportNotSupported(Subject, target, StackSemantics.Value);
            return;
        }
        if (Type.IsArgumentSplice())
        {
            AstArgumentSplice.ReportNotSupported(Type, target, StackSemantics.Value);
            return;
        }

        var argv = AstPartiallyApplicable.PreprocessPartialApplicationArguments(
            Subject.Singleton()
        );
        var ctorArgc = AstPartiallyApplicable.EmitConstructorArguments(this, target, argv);
        if (Type is AstConstantTypeExpression constType)
            target.EmitConstant(Position, constType.TypeExpression);
        else
            Type.EmitValueCode(target);

        target.EmitCommandCall(Position, ctorArgc + 1, Engine.PartialTypeCastAlias);
    }
}
