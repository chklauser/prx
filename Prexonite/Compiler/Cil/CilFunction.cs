

namespace Prexonite.Compiler.Cil;

/// <summary>
///     A managed implementation of a <see cref = "PFunction" />s byte code.
/// </summary>
/// <param name = "source">A reference to the original function. (The source code)</param>
/// <param name = "sctx">The stack context used by the calling function.</param>
/// <param name = "args">An array of arguments. Must not be null, but may be empty.</param>
/// <param name = "sharedVariables">An array of variables shared with the callee. May be null if no variables are shared.</param>
/// <param name = "returnValue">Will hold the value returned by the function. Will never be a null reference.</param>
/// <param name = "returnMode">Will hold the return mode chosen by the function.</param>
[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
    MessageId = nameof(Cil))]
public delegate void CilFunction(
    PFunction source, StackContext sctx, PValue[] args, PVariable[]? sharedVariables,
    out PValue returnValue, out ReturnMode returnMode);