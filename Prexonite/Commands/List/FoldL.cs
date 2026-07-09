

using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.List;

/// <summary>
///     Implementation of the foldl function.
/// </summary>
/// <remarks>
///     <code>function foldl(ref f, left, source)
///         {
///         foreach(var right in source)
///         left = f(left,right);
///         return left;
///         }</code>
/// </remarks>
public class FoldL : PCommand, ICilCompilerAware
{
    #region Singleton

    FoldL()
    {
    }

    public static FoldL Instance { get; } = new();

    #endregion

    public static PValue Run(
        StackContext sctx, IIndirectCall f, PValue left, IEnumerable<PValue> source)
    {
        if (sctx == null)
            throw new ArgumentNullException(nameof(sctx));
        if (f == null)
            throw new ArgumentNullException(nameof(f));

        foreach (var right in source)
        {
            left = f.IndirectCall(sctx, left, right);
        }
        return left;
    }

    public static PValue RunStatically(StackContext sctx, PValue[] args)
    {
        if (sctx == null)
            throw new ArgumentNullException(nameof(sctx));
        if (args == null)
            throw new ArgumentNullException(nameof(args));

        //Get f
        IIndirectCall f;
        if (args.Length < 1)
            throw new PrexoniteException("The foldl command requires a function argument.");
        else
            f = args[0];

        //Get left
        var left = args.Length < 2 ? null : args[1];

        //Get the source
        IEnumerable<PValue> source;
        if (args.Length == 3)
        {
            var psource = args[2];
            source = Map._ToEnumerable(sctx, psource);
        }
        else
        {
            var lstsource = new List<PValue>();
            for (var i = 2; i < args.Length; i++)
            {
                var multiple = Map._ToEnumerable(sctx, args[i]);
                lstsource.AddRange(multiple);
            }
            source = lstsource;
        }

        return Run(sctx, f, left ?? PType.Null, source);
    }

    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args)
    {
        return RunStatically(sctx, args.ToArray());
    }

    #region ICilCompilerAware Members

    /// <summary>
    ///     Asses qualification and preferences for a certain instruction.
    /// </summary>
    /// <param name = "ins">The instruction that is about to be compiled.</param>
    /// <returns>A set of <see cref = "CompilationFlags" />.</returns>
    CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
    {
        return CompilationFlags.PrefersRunStatically;
    }

    /// <summary>
    ///     Provides a custom compiler routine for emitting CIL byte code for a specific instruction.
    /// </summary>
    /// <param name = "state">The compiler state.</param>
    /// <param name = "ins">The instruction to compile.</param>
    void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
    {
        throw new NotSupportedException();
    }

    #endregion
}