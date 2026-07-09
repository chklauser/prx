

using System.Diagnostics;

namespace Prexonite.Compiler;

/// <summary>
///     A method that modifies the supplied 
///     <see cref = "CompilerTarget" /> when invoked prior to optimization and code generation.
/// </summary>
/// <param name = "target">The <see cref = "CompilerTarget" /> of the function to be modified.</param>
public delegate void AstTransformation(CompilerTarget target);

/// <summary>
///     Union class for both managed as well as interpreted compiler hooks.
/// </summary>
[DebuggerNonUserCode]
public sealed class CompilerHook
{
    readonly AstTransformation? _managed;
    readonly PValue? _interpreted;

    /// <summary>
    ///     Creates a new compiler hook, that executes a managed method.
    /// </summary>
    /// <param name = "transformation">A managed transformation.</param>
    public CompilerHook(AstTransformation transformation)
    {
        _managed = transformation ?? throw new ArgumentNullException(nameof(transformation));
    }

    /// <summary>
    ///     Creates a new compiler hook, that indirectly calls a <see cref = "PValue" />.
    /// </summary>
    /// <param name = "transformation">A value that supports indirect calls (such as a function reference).</param>
    public CompilerHook(PValue transformation)
    {
        _interpreted = transformation ?? throw new ArgumentNullException(nameof(transformation));
    }

    /// <summary>
    ///     Indicates whether the compiler hook is managed.
    /// </summary>
    [MemberNotNullWhen(true, nameof(_managed))]
    [MemberNotNullWhen(false, nameof(_interpreted))]
    public bool IsManaged => _managed != null;

    /// <summary>
    ///     Indicates whether the compiler hook is interpreted.
    /// </summary>
    [MemberNotNullWhen(true, nameof(_interpreted))]
    [MemberNotNullWhen(false, nameof(_managed))]
    public bool IsInterpreted => _interpreted != null;

    /// <summary>
    ///     Executes the compiler hook (either calls the managed 
    ///     delegate or indirectly calls the <see cref = "PValue" /> in the context of the <see cref = "Loader" />.)
    /// </summary>
    /// <param name = "target">The compiler target to modify.</param>
    public void Execute(CompilerTarget target)
    {
        try
        {
            target.Loader.ParentApplication._SuppressInitialization = true;
            if (IsManaged)
                _managed(target);
            else
                _interpreted.IndirectCall(
                    target.Loader,
                    target.Loader.CreateNativePValue(target));
        }
        finally
        {
            target.Loader.ParentApplication._SuppressInitialization = false;
        }
    }
}