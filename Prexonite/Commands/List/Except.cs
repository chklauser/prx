using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.List;

public class Except : PCommand, ICilCompilerAware
{
    #region Singleton pattern

    public static Except Instance { get; } = new();

    Except() { }

    #endregion

    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args)
    {
        return RunStatically(sctx, args.ToArray());
    }

    public static PValue RunStatically(StackContext sctx, PValue[] args)
    {
        if (args == null)
            throw new ArgumentNullException(nameof(args));
        if (sctx == null)
            throw new ArgumentNullException(nameof(sctx));

        var xss = new List<IEnumerable<PValue>>();
        foreach (var arg in args)
        {
            var xs = Map._ToEnumerable(sctx, arg);
            if (xs != null)
                xss.Add(xs);
        }

        var n = xss.Count;
        if (n < 2)
            throw new PrexoniteException("Except requires at least two sources.");

        var t = new Dictionary<PValue, bool>();
        //All elements of the last source are considered candidates
        foreach (var x in xss[n - 1])
            if (!t.ContainsKey(x))
                t.Add(x, true);

        for (var i = 0; i < n - 1; i++)
            foreach (var x in xss[i])
                if (t.ContainsKey(x))
                    t.Remove(x);

        return sctx.CreateNativePValue(t.Keys);
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
