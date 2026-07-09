

using System.Text;
using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.Text;

public class SetCenterCommand : PCommand, ICilCompilerAware
{
    #region Singleton

    SetCenterCommand()
    {
    }

    public static SetCenterCommand Instance { get; } = new();

    #endregion

    public static PValue RunStatically(StackContext sctx, PValue[] args)
    {
        // function setright(w,s,f)
        if (sctx == null)
            throw new ArgumentNullException(nameof(sctx));
        args ??= [];

        string s;
        int w;
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
        w = (int) args[0].ConvertTo(sctx, PType.Int).Value!;
        if (args.Length > 2)
            f = args[2].CallToString(sctx);
        else
            f = " ";

        var l = s.Length;
        if (l >= w)
            return s;

        var sb = new StringBuilder(w);

        var lw = (int) System.Math.Round(w / 2.0, 0, MidpointRounding.AwayFromZero);
        var rw = w - lw;

        var ll = (int) System.Math.Round(l / 2.0, 0, MidpointRounding.AwayFromZero);

        sb.Append(SetRightCommand.SetRight(lw, s[..ll], f));
        sb.Append(SetLeftCommand.SetLeft(rw, s[ll..], f));
        return sb.ToString();
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