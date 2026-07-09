

namespace Prexonite.Compiler.Ast;

public class AstTypecheck : AstExpr,
    IAstHasExpressions,
    IAstPartiallyApplicable
{
    AstExpr _subject;

    public AstTypecheck(
        ISourcePosition position, AstExpr subject, AstTypeExpr type)
        : base(position)
    {
        _subject = subject ?? throw new ArgumentNullException(nameof(subject));
        Type = type ?? throw new ArgumentNullException(nameof(type));
    }

    #region IAstHasExpressions Members

    public AstExpr[] Expressions
    {
        get { return [_subject]; }
    }

    public AstExpr Subject
    {
        get => _subject;
        set => _subject = value;
    }

    public AstTypeExpr Type { get; set; }

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

        _subject.EmitValueCode(target);
        if (Type is AstConstantTypeExpression constType)
        {
            PType? T = null;
            try
            {
                T = target.Loader.ConstructPType(constType.TypeExpression);
            }
            catch (PrexoniteException)
            {
                //ignore failures here
            }
            if ((object?) T != null && T == PType.Null)
                target.Emit(Position,OpCode.check_null);
            else
                target.Emit(Position,OpCode.check_const, constType.TypeExpression);
        }
        else
        {
            Type.EmitValueCode(target);
            target.Emit(Position,OpCode.check_arg);
        }
    }

    public override bool TryOptimize(CompilerTarget target, [NotNullWhen(true)] out AstExpr? expr)
    {
        _OptimizeNode(target, ref _subject);
        Type = (AstTypeExpr) _GetOptimizedNode(target, Type);

        expr = null;

        if (_subject is not AstConstant constSubject || Type is not AstConstantTypeExpression constType)
            return false;
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
        expr =
            new AstConstant(File, Line, Column, constSubject.ToPValue(target).Type.Equals(type));
        return true;
    }
        
    public NodeApplicationState CheckNodeApplicationState()
    {
        return new(Subject.IsPlaceholder() || Type.IsPlaceholder(), 
            Subject.IsArgumentSplice() || Type.IsArgumentSplice());
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
            
        var argv =
            AstPartiallyApplicable.PreprocessPartialApplicationArguments(_subject.Singleton());
        var ctorArgc = AstPartiallyApplicable.EmitConstructorArguments(this, target, argv);
        if (Type is AstConstantTypeExpression constType)
            target.EmitConstant(Position, constType.TypeExpression);
        else
            Type.EmitValueCode(target);

        target.EmitCommandCall(Position, ctorArgc + 1, Engine.PartialTypeCheckAlias);
    }
}