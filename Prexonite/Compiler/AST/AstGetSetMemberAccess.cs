using JetBrains.Annotations;

namespace Prexonite.Compiler.Ast;

[method: PublicAPI]
public class AstGetSetMemberAccess(
    string file,
    int line,
    int column,
    PCall call,
    AstExpr subject,
    string id
) : AstGetSetImplBase(file, line, column, call), IAstPartiallyApplicable
{
    public string Id { get; set; } = id;
    public AstExpr Subject { get; set; } = subject;

    public override AstExpr[] Expressions
    {
        get
        {
            var len = Arguments.Count;
            var ary = new AstExpr[len + 1];
            Array.Copy(Arguments.ToArray(), 0, ary, 1, len);
            ary[0] = Subject;
            return ary;
        }
    }

    [PublicAPI]
    public AstGetSetMemberAccess(string file, int line, int column, AstExpr subject, string id)
        : this(file, line, column, PCall.Get, subject, id) { }

    public override int DefaultAdditionalArguments => base.DefaultAdditionalArguments + 1;

    protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
    {
        Subject.EmitValueCode(target);
        base.DoEmitCode(target, stackSemantics);
    }

    protected override void EmitGetCode(CompilerTarget target, StackSemantics stackSemantics)
    {
        target.EmitGetCall(Position, Arguments.Count, Id, stackSemantics == StackSemantics.Effect);
    }

    protected override void EmitSetCode(CompilerTarget target)
    {
        target.EmitSetCall(Position, Arguments.Count, Id);
    }

    public override bool TryOptimize(CompilerTarget target, [NotNullWhen(true)] out AstExpr? expr)
    {
        base.TryOptimize(target, out expr);
        var subject = Subject;
        _OptimizeNode(target, ref subject);
        Subject = subject;
        return false;
    }

    public override AstGetSet GetCopy()
    {
        var copy = new AstGetSetMemberAccess(File, Line, Column, Call, Subject, Id);
        CopyBaseMembers(copy);
        return copy;
    }

    public override string ToString()
    {
        var name = Enum.GetName(typeof(PCall), Call);
        return $"{name?.ToLowerInvariant() ?? "-"}: ({Subject}).{Id}{ArgumentsToString()}";
    }

    #region Implementation of IAstPartiallyApplicable

    public void DoEmitPartialApplicationCode(CompilerTarget target)
    {
        var argv = AstPartiallyApplicable.PreprocessPartialApplicationArguments(
            Subject.Singleton().Append(Arguments)
        );
        var ctorArgc = AstPartiallyApplicable.EmitConstructorArguments(this, target, argv);
        target.EmitConstant(Position, (int)Call);
        target.EmitConstant(Position, Id);
        target.EmitCommandCall(Position, ctorArgc + 2, Engine.PartialMemberCallAlias);
    }

    public override NodeApplicationState CheckNodeApplicationState()
    {
        var state = base.CheckNodeApplicationState();
        return state.WithPlaceholders(state.HasPlaceholders || Subject.IsPlaceholder());
    }

    #endregion
}
