using System.Diagnostics;

namespace Prexonite.Compiler.Macro;

/// <summary>
///     Interface for commands that are applied at compile-time.
/// </summary>
public abstract class MacroCommand
{
    /// <summary>
    ///     Creates a new instance of the macro command. It will identify itself with the supplied id.
    /// </summary>
    /// <param name = "id">The name of the physical slot, this command resides in.</param>
    protected MacroCommand(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("MacroCommand.Id must not be null or empty.");
        Id = id;
    }

    /// <summary>
    ///     ID (slot name) of this macro command.
    /// </summary>
    public string Id { [DebuggerStepThrough] get; }

    /// <summary>
    ///     Implementation of the application of this macro.
    /// </summary>
    /// <param name = "context">The macro context for this macro expansion.</param>
    protected abstract void DoExpand(MacroContext context);

    /// <summary>
    ///     Expands the macro according to the supplied macro context.
    /// </summary>
    /// <param name = "context">Supplies call site information to the macro.</param>
    public void Expand(MacroContext context)
    {
        DoExpand(context);
    }
}
