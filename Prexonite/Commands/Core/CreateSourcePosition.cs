using Prexonite.Compiler;
using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.Core;

public class CreateSourcePosition : PCommand, ICilCompilerAware
{
    #region Singleton

    public static CreateSourcePosition Instance { get; } = new();

    CreateSourcePosition() { }

    public const string Alias = "create_source_position";

    #endregion

    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args)
    {
        return RunStatically(sctx, args.ToArray());
    }

    public static PValue RunStatically(StackContext sctx, PValue[] args)
    {
        if (args.Length == 0)
        {
            return sctx.CreateNativePValue(NoSourcePosition.Instance);
        }

        var file = args[0].CallToString(sctx);
        int? line,
            column;

        if (args.Length >= 2 && args[1].TryConvertTo(sctx, IntPType.Instance, true, out var box))
        {
            line = (int)box.Value!;
        }
        else
        {
            line = null;
        }

        if (args.Length >= 3 && args[2].TryConvertTo(sctx, IntPType.Instance, true, out box))
        {
            column = (int)box.Value!;
        }
        else
        {
            column = null;
        }

        return sctx.CreateNativePValue(new SourcePosition(file, line ?? -1, column ?? -1));
    }

    public CompilationFlags CheckQualification(Instruction ins)
    {
        return CompilationFlags.PrefersRunStatically;
    }

    public void ImplementInCil(CompilerState state, Instruction ins)
    {
        throw new NotSupportedException(
            "The command " + Alias + " does not provide a custom CIL implementation."
        );
    }
}
