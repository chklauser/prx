namespace Prexonite.Commands.Core;

/// <summary>
///     Implementation of the <c>print</c> command.
/// </summary>
public class DynamicPrint : PCommand
{
    readonly TextWriter _writer;

    /// <summary>
    ///     Creates a new <c>println</c> command, that prints to the supplied <see cref = "TextWriter" />.
    /// </summary>
    /// <param name = "writer">The TextWriter to write to.</param>
    public DynamicPrint(TextWriter writer)
    {
        _writer = writer;
    }

    /// <summary>
    ///     Creates a new <c>println</c> command that prints to <see cref = "Console.Out" />.
    /// </summary>
    public DynamicPrint()
    {
        _writer = Console.Out;
    }

    /// <summary>
    ///     Prints all arguments.
    /// </summary>
    /// <param name = "sctx">The context in which to convert the arguments to strings.</param>
    /// <param name = "args">The list of arguments to print.</param>
    /// <returns></returns>
    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args)
    {
        var s = Concat.ConcatenateString(sctx, args);
        _writer.Write(s);

        return s;
    }
}
