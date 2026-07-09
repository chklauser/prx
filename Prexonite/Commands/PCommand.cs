

namespace Prexonite.Commands;

/// <summary>
///     The abstract base class for all commands (built-in functions)
/// </summary>
public abstract class PCommand : IIndirectCall
{
    /// <summary>
    ///     Executes the command.
    /// </summary>
    /// <param name = "sctx">The stack context in which to execut the command.</param>
    /// <param name = "args">The arguments to be passed to the command.</param>
    /// <returns>The value returned by the command. Must not be null. (But possibly {null~Null})</returns>
    public abstract PValue Run(StackContext sctx, ReadOnlySpan<PValue> args);
    
    public PValue Run(StackContext sctx, params PValue[] args) => Run(sctx, new ReadOnlySpan<PValue>(args));

    #region IIndirectCall Members

    /// <summary>
    ///     Runs the command. (Calls <see cref = "Run(Prexonite.StackContext,System.ReadOnlySpan{Prexonite.PValue})" />)
    /// </summary>
    /// <param name = "sctx">The stack context in which to call the command.</param>
    /// <param name = "args">The arguments to pass to the command.</param>
    /// <returns>The value returned by the command.</returns>
    PValue IIndirectCall.IndirectCall(StackContext sctx, params ReadOnlySpan<PValue> args)
    {
        return Run(sctx, args) ?? PType.Null.CreatePValue();
    }

    #endregion
}

/// <summary>
///     Defines command spaces (or groups).
/// </summary>
[Flags]
public enum PCommandGroups
{
    /// <summary>
    ///     No command group.
    /// </summary>
    None = 0,

    /// <summary>
    ///     The command group reserved for built-in commands. Supplied by the Prexonite VM.
    /// </summary>
    Engine = 1,

    /// <summary>
    ///     The command group for commands provided by the host application.
    /// </summary>
    Host = 2,

    /// <summary>
    ///     The command group for commands added by user code (script code).
    /// </summary>
    User = 4,

    /// <summary>
    ///     The command group for commands added by the compiler (for the build block).
    /// </summary>
    Compiler = 8,
}