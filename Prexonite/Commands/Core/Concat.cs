

using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.Core;

/// <summary>
///     Implementation of the <c>concat</c> command.
/// </summary>
public sealed class Concat : PCommand, ICilCompilerAware
{
    #region Singleton pattern

    Concat()
    {
    }

    public static Concat Instance { get; } = new();

    #endregion

    /// <summary>
    ///     Concatenates all arguments and return one big string.
    /// </summary>
    /// <param name = "sctx">The context in which to convert the arguments to strings.</param>
    /// <param name = "args">The list of fragments to concatenate.</param>
    /// <returns>The concatenated string.</returns>
    /// <remarks>
    ///     Please note that this method uses a string builder. The addition operator is faster for only two fragments.
    /// </remarks>
    public static string ConcatenateString(StackContext sctx, ReadOnlySpan<PValue> args)
    {
        var elements = new string[args.Length];
        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            var element = arg.Type is StringPType ? (string) arg.Value! : arg.CallToString(sctx);
            elements[i] = element;
        }

        return string.Concat(elements);
    }

    /// <summary>
    ///     Concatenates all arguments and return one big string.
    /// </summary>
    /// <param name = "sctx">The context in which to convert the arguments to strings.</param>
    /// <param name = "args">The list of fragments to concatenate.</param>
    /// <returns>The concatenated string.</returns>
    /// <remarks>
    ///     Please note that this method uses a string builder. The addition operator is faster for only two fragments.
    /// </remarks>
    public static PValue RunStatically(StackContext sctx, PValue[] args)
    {
        return ConcatenateString(sctx, args);
    }

    /// <summary>
    ///     Concatenates all arguments and return one big string.
    /// </summary>
    /// <param name = "sctx">The context in which to convert the arguments to strings.</param>
    /// <param name = "args">The list of fragments to concatenate.</param>
    /// <returns>A PValue containing the concatenated string.</returns>
    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args)
    {
        return RunStatically(sctx, args.ToArray());
    }

    #region ICilCompilerAware Members

    CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
    {
        return CompilationFlags.PrefersRunStatically;
    }

    void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
    {
        throw new NotSupportedException();
    }

    #endregion
}