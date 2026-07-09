

using System.Text;
using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.Text;

public class SetLeftCommand : PCommand, ICilCompilerAware
{
    #region Singleton

    SetLeftCommand()
    {
    }

    public static SetLeftCommand Instance { get; } = new();

    #endregion

    public static PValue RunStatically(StackContext sctx, PValue[] args)
    {
        // function setright(w,s,f)
        if (sctx == null)
            throw new ArgumentNullException(nameof(sctx));

        string s;
        string f;

        switch (args.Length)
        {
            case 0:
                return "";
            case 1:
                s = "";
                goto parseW;
        }
        s = args[1].CallToString(sctx);
        parseW:
        var w = (int) args[0].ConvertTo(sctx, PType.Int).Value!;
        if (args.Length > 2)
            f = args[2].CallToString(sctx);
        else
            f = " ";

        return SetLeft(w, s, f);
    }

    public static string SetLeft(int w, string s, string f)
    {
        var fl = f.Length;
        var l = s.Length;
        if (l >= w)
            return s;

        var sb = new StringBuilder(w, w);
        sb.Append(s);

        for (; l < w; l += fl)
            sb.Append(f);
        sb.Length = w;
        return sb.ToString();
    }

    public static string SetLeft(int w, string s)
    {
        return SetLeft(w, s, " ");
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