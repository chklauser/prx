namespace Prexonite.Commands;

/// <summary>
///     Implementation of <see cref = "PCommand" /> that forwards the run call to
///     a class that implements <see cref = "ICommand" />.
/// </summary>
/// <seealso cref = "PCommand" />
/// <seealso cref = "ICommand" />
public sealed class NestedPCommand : PCommand
{
    /// <summary>
    ///     Provides access to the implementation of this specific instance of <see cref = "NestedPCommand" />.
    /// </summary>
    public ICommand Action { get; }

    /// <summary>
    ///     Creates a new <see cref = "NestedPCommand" />.
    /// </summary>
    /// <param name = "action">Any implementation of <see cref = "ICommand" />.</param>
    /// <exception cref = "ArgumentNullException"><paramref name = "action" /> is null.</exception>
    public NestedPCommand(ICommand action)
    {
        Action = action ?? throw new ArgumentNullException(nameof(action));
    }

    /// <summary>
    ///     Executes <see cref = "ICommand.Run" /> on <see cref = "Action" />.
    /// </summary>
    /// <param name = "sctx">The stack context in which to execute the command.</param>
    /// <param name = "args">The arguments to pass to the command invocation.</param>
    /// <returns>The value returned by <c><see cref = "Action" />.Run</c>.</returns>
    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args)
    {
        return Action.Run(sctx, args);
    }

    /// <summary>
    ///     Returns a description of the nested command instance.
    /// </summary>
    /// <returns>A description of the nested command instance.</returns>
    public override string ToString()
    {
        return "Nested(" + Action + ")";
    }
}

/// <summary>
///     Interface to be implemented by a class to be used as a command.
/// </summary>
/// <seealso cref = "PCommand" />
/// <seealso cref = "NestedPCommand" />
/// <remarks>
///     In order to be used as a command, <see cref = "NestedPCommand" /> need to be wrapped around instances of types that implement this interface.
/// </remarks>
public interface ICommand
{
    /// <summary>
    ///     Actual implementation of a command.
    /// </summary>
    /// <param name = "sctx">The stack context in which the command is executed.</param>
    /// <param name = "args">The array of arguments supplied to the command.</param>
    /// <returns>The value returned by the command.</returns>
    /// <remarks>
    ///     If your implementation does not return a value, you have to return <c>PType.Null.CreatePValue()</c> and <strong>not</strong> <c>null</c>!
    /// </remarks>
    PValue Run(StackContext sctx, ReadOnlySpan<PValue> args);
}
