using System.Diagnostics;
using System.Reflection;

namespace Prexonite;

[
    SuppressMessage(
        "Microsoft.Naming",
        "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = "Cil"
    ),
    DebuggerStepThrough
]
public sealed class CilFunctionContext : StackContext
{
    public static CilFunctionContext New(StackContext caller, PFunction originalImplementation)
    {
        if (originalImplementation == null)
            throw new ArgumentNullException(nameof(originalImplementation));
        return new(
            caller.ParentEngine,
            originalImplementation.ParentApplication,
            originalImplementation.ImportedNamespaces
        );
    }

    internal static MethodInfo NewMethod { get; } =
        typeof(CilFunctionContext).GetMethod(
            nameof(New),
            [typeof(StackContext), typeof(PFunction)]
        )!;

    CilFunctionContext(
        Engine parentEngine,
        Application parentApplication,
        SymbolCollection importedNamespaces
    )
    {
        ParentEngine = parentEngine ?? throw new ArgumentNullException(nameof(parentEngine));
        ParentApplication =
            parentApplication ?? throw new ArgumentNullException(nameof(parentApplication));
        ImportedNamespaces =
            importedNamespaces ?? throw new ArgumentNullException(nameof(importedNamespaces));
    }

    /// <summary>
    ///     Represents the engine this context is part of.
    /// </summary>
    public override Engine ParentEngine { get; }

    /// <summary>
    ///     The parent application.
    /// </summary>
    public override Application ParentApplication { get; }

    public override SymbolCollection ImportedNamespaces { get; }

    /// <summary>
    ///     Indicates whether the context still has code/work to do.
    /// </summary>
    /// <returns>True if the context has additional work to perform in the next cycle, False if it has finished it's work and can be removed from the stack</returns>
    protected override bool PerformNextCycle(StackContext? lastContext)
    {
        return false;
    }

    /// <summary>
    ///     Tries to handle the supplied exception.
    /// </summary>
    /// <param name = "exc">The exception to be handled.</param>
    /// <returns>True if the exception has been handled, false otherwise.</returns>
    public override bool TryHandleException(Exception exc)
    {
        return false;
    }

    /// <summary>
    ///     Represents the return value of the context.
    ///     Just providing a value here does not mean that it gets consumed by the caller.
    ///     If the context does not provide a return value, this property should return null (not NullPType).
    /// </summary>
    public override PValue ReturnValue => PType.Null;
}
