using System.Diagnostics;

namespace Prexonite.Compiler.Ast;

public class AstObjectCreation : AstExpr, IAstHasExpressions, IAstPartiallyApplicable
{
    public ArgumentsProxy Arguments { get; }

    #region IAstHasExpressions Members

    public AstExpr[] Expressions => Arguments.ToArray();

    public AstTypeExpr TypeExpr { get; set; }

    #endregion

    readonly List<AstExpr> _arguments = new();

    public AstObjectCreation(ISourcePosition position, AstTypeExpr type)
        : base(position)
    {
        TypeExpr = type ?? throw new ArgumentNullException(nameof(type));
        Arguments = new(_arguments);
    }

    [Obsolete]
    [DebuggerStepThrough]
    public AstObjectCreation(string file, int line, int col, AstTypeExpr type)
        : this(new SourcePosition(file, line, col), type) { }

    [DebuggerStepThrough]
    internal AstObjectCreation(Parser p, AstTypeExpr type)
        : this(p.GetPosition(), type) { }

    #region AstExpr Members

    public override bool TryOptimize(CompilerTarget target, [NotNullWhen(true)] out AstExpr? expr)
    {
        expr = null;

        TypeExpr = (AstTypeExpr)_GetOptimizedNode(target, TypeExpr);

        //Optimize arguments
        for (var i = 0; i < _arguments.Count; i++)
        {
            var arg = _arguments[i];
            var oArg = _GetOptimizedNode(target, arg);
            if (ReferenceEquals(oArg, arg))
                continue;
            _arguments[i] = oArg;
        }

        return false;
    }

    protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
    {
        if (TypeExpr is AstConstantTypeExpression constType)
        {
            foreach (var arg in _arguments)
                arg.EmitValueCode(target);
            target.Emit(Position, OpCode.newobj, _arguments.Count, constType.TypeExpression);
            if (stackSemantics == StackSemantics.Effect)
                target.Emit(Position, Instruction.CreatePop());
        }
        else
        {
            //Load type and call construct on it
            TypeExpr.EmitValueCode(target);
            foreach (var arg in _arguments)
                arg.EmitValueCode(target);
            var justEffect = stackSemantics == StackSemantics.Effect;
            target.EmitGetCall(Position, _arguments.Count, PType.ConstructFromStackId, justEffect);
        }
    }

    #endregion

    #region Implementation of IAstPartiallyApplicable

    public NodeApplicationState CheckNodeApplicationState()
    {
        return new(
            TypeExpr.IsPlaceholder() || Arguments.Any(x => x.IsPlaceholder()),
            TypeExpr.IsArgumentSplice() || Arguments.Any(x => x.IsArgumentSplice())
        );
    }

    public void DoEmitPartialApplicationCode(CompilerTarget target)
    {
        var argv = AstPartiallyApplicable.PreprocessPartialApplicationArguments(Arguments.ToList());
        var ctorArgc = AstPartiallyApplicable.EmitConstructorArguments(this, target, argv);
        if (TypeExpr is AstConstantTypeExpression constType)
            target.EmitConstant(Position, constType.TypeExpression);
        else
            TypeExpr.EmitValueCode(target);
        target.EmitCommandCall(Position, ctorArgc + 1, Engine.PartialConstructionAlias);
    }

    #endregion
}
