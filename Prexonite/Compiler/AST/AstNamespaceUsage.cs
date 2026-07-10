using Prexonite.Compiler.Symbolic;
using Prexonite.Properties;

namespace Prexonite.Compiler.Ast;

/// <summary>
/// A reference to a namespace. Used temporarily while resolving qualified names.
/// </summary>
/// <remarks>
/// <para>Does not have a meaningful translation into executable code,
/// but is available for diagnostic and meta programming purposes</para>
/// <para>
/// Naked namespace usages (without a dot following them) can be used as arguments to macros.
/// Such macros need to be prepared to handle namespace references.
/// </para>
/// <para>Trying to generate code for a naked namespace usage always fails.</para>
/// </remarks>
public class AstNamespaceUsage : AstGetSetImplBase
{
    QualifiedId? _referencePath;

    public AstNamespaceUsage(ISourcePosition position, PCall call, Namespace @namespace)
        : base(position, call)
    {
        Namespace = @namespace ?? throw new ArgumentNullException(nameof(@namespace));
    }

    /// <summary>
    /// Represents the path that was used to access this namespace usage. Might be null.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This property optionally assigned by the parser when a namespace is accessed via a qualified path.
    /// It only consists of the qualified id as it appears at the usage site.
    /// </para>
    /// <para>
    /// The reference path does not include prefixes imported into the current scope.
    /// Consequently, it needs to be evaluated in the same context/scope that produced it,
    /// otherwise it is meaningless.
    /// </para>
    /// </remarks>
    public QualifiedId? ReferencePath
    {
        get => _referencePath;
        set
        {
            if (_referencePath != null)
                throw new InvalidOperationException(
                    "Can only assign namespace usage reference path once."
                );
            _referencePath = value;
        }
    }

    public Namespace Namespace { get; }

    protected override void EmitGetCode(CompilerTarget target, StackSemantics stackSemantics)
    {
        target.Loader.ReportMessage(
            Message.Error(
                Resources.Parser_ExpectedEntityFoundNamespace,
                Position,
                MessageClasses.ExpectedEntityFoundNamespace
            )
        );
        if (stackSemantics == StackSemantics.Value)
            target.EmitNull(Position);
    }

    protected override void EmitSetCode(CompilerTarget target)
    {
        EmitGetCode(target, StackSemantics.Effect);
    }

    public override AstGetSet GetCopy()
    {
        var copy = new AstNamespaceUsage(Position, Call, Namespace);
        if (ReferencePath != null)
            copy.ReferencePath = ReferencePath;
        return copy;
    }
}
