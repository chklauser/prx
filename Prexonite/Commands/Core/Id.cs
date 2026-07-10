using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.Core;

public class Id : PCommand, ICilCompilerAware
{
    #region Singleton pattern

    public static Id Instance { get; } = new();

    Id() { }

    #endregion

    public const string Alias = "id";

    public static PValue RunStatically(StackContext sctx, PValue[] args)
    {
        return args.Length > 0 ? args[0] : PType.Null;
    }

    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args)
    {
        return RunStatically(sctx, args.ToArray());
    }

    public CompilationFlags CheckQualification(Instruction ins)
    {
        return CompilationFlags.PrefersRunStatically;
    }

    public void ImplementInCil(CompilerState state, Instruction ins)
    {
        var argc = ins.Arguments;
        if (argc == 0)
            return;

        if (ins.JustEffect)
        {
            state.EmitIgnoreArguments(argc);
        }
        else
        {
            state.EmitIgnoreArguments(argc - 1);
        }
    }
}
