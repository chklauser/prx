using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.List;

/// <summary>
///     Implementation of the foldr function.
/// </summary>
/// <remarks>
///     <code>function foldr(ref f, right, source)
///         {
///         var lst = [];
///         foreach(var e in source)
///             lst[] = e;
///         for(var i = lst.Count-1; i>=0; i--)
///             right = f(lst[i],right);
///         return right;
///         }</code>
/// </remarks>
public class FoldR : PCommand, ICilCompilerAware
{
    #region Singleton

    FoldR() { }

    public static FoldR Instance { get; } = new();

    #endregion

    public static PValue Run(
        StackContext sctx,
        IIndirectCall f,
        PValue right,
        IEnumerable<PValue> source
    )
    {
        if (sctx == null)
            throw new ArgumentNullException(nameof(sctx));
        if (f == null)
            throw new ArgumentNullException(nameof(f));

        var lst = new List<PValue>(source);

        for (var i = lst.Count - 1; i >= 0; i--)
        {
            right = f.IndirectCall(sctx, lst[i], right);
        }
        return right;
    }

    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args)
    {
        return RunStatically(sctx, args.ToArray());
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
            throw new PrexoniteException("The foldr command requires a function argument.");
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
            for (var i = 1; i < args.Length; i++)
            {
                var multiple = Map._ToEnumerable(sctx, args[i]);
                lstsource.AddRange(multiple);
            }
            source = lstsource;
        }

        return Run(sctx, f, left ?? PType.Null, source);
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
