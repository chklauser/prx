using System.Reflection;
using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.Core.PartialApplication;

public class FunctionalPartialCallCommand : PCommand, ICilExtension
{
    #region Singleton pattern

    public static FunctionalPartialCallCommand Instance { get; } = new();

    FunctionalPartialCallCommand() { }

    public const string Alias = @"pa\fun\call";

    #endregion

    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args)
    {
        if (args.Length < 1)
            return PType.Null;

        var closed = new PValue[args.Length - 1];
        args[1..].CopyTo(closed.AsSpan(0, args.Length - 1));
        return sctx.CreateNativePValue(new FunctionalPartialCall(args[0], closed));
    }

    bool ICilExtension.ValidateArguments(CompileTimeValue[] staticArgv, int dynamicArgc)
    {
        return true;
    }

    ConstructorInfo? _functionPartialCallCtorCache;

    ConstructorInfo functionPartialCallCtor
    {
        get
        {
            return _functionPartialCallCtorCache ??=
                typeof(FunctionalPartialCall).GetConstructor([typeof(PValue), typeof(PValue[])])
                ?? throw new InvalidOperationException(
                    $"Could not find constructor for {nameof(FunctionalPartialCall)} with (PValue, PValue[])."
                );
        }
    }

    void ICilExtension.Implement(
        CompilerState state,
        Instruction ins,
        CompileTimeValue[] staticArgv,
        int dynamicArgc
    )
    {
        FlippedFunctionalPartialCallCommand._ImplementCtorCall(
            state,
            ins,
            staticArgv,
            dynamicArgc,
            functionPartialCallCtor
        );
    }
}
