namespace Prexonite.Commands.Core;

/// <summary>
///     Implementation of <c>println</c>
/// </summary>
public class DynamicPrintLine : PCommand
{
    readonly TextWriter _writer;

    /// <summary>
    ///     Creates a new <c>println</c> command, that prints to the supplied <see cref = "TextWriter" />.
    /// </summary>
    /// <param name = "writer">The TextWriter to write to.</param>
    public DynamicPrintLine(TextWriter writer)
    {
        _writer = writer;
    }

    /// <summary>
    ///     Creates a new <c>println</c> command that prints to <see cref = "Console.Out" />.
    /// </summary>
    public DynamicPrintLine()
    {
        _writer = Console.Out;
    }

    /// <summary>
    ///     Prints all arguments and appends a NewLine.
    /// </summary>
    /// <param name = "sctx">The context in which to convert the arguments to strings.</param>
    /// <param name = "args">The list of arguments to print.</param>
    /// <returns></returns>
    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args)
    {
        var s = Concat.ConcatenateString(sctx, args);

        _writer.WriteLine(s);

        return s;
    }
}
