using System.Reflection;
using System.Reflection.Emit;
using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.Core.PartialApplication;

public class FlippedFunctionalPartialCallCommand : PCommand, ICilExtension
{
    #region Singleton pattern

    public static FlippedFunctionalPartialCallCommand Instance { get; } = new();

    FlippedFunctionalPartialCallCommand() { }

    public const string Alias = @"pa\flip\call";

    #endregion

    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args)
    {
        if (args.Length < 1)
            return PType.Null;

        var closed = new PValue[args.Length - 1];
        args[1..].CopyTo(closed.AsSpan(0, args.Length - 1));
        return sctx.CreateNativePValue(new FlippedFunctionalPartialCall(args[0], closed));
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
                typeof(FlippedFunctionalPartialCall).GetConstructor([
                    typeof(PValue),
                    typeof(PValue[]),
                ])
                ?? throw new InvalidOperationException(
                    $"Could not find constructor for {nameof(FlippedFunctionalPartialCall)} with (PValue, PValue[])."
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
        _ImplementCtorCall(state, ins, staticArgv, dynamicArgc, functionPartialCallCtor);
    }

    internal static void _ImplementCtorCall(
        CompilerState state,
        Instruction ins,
        CompileTimeValue[] staticArgv,
        int dynamicArgc,
        ConstructorInfo partialCallCtor
    )
    {
        //the call subject is not part of argv
        var argc = staticArgv.Length + dynamicArgc - 1;

        if (argc == 0)
        {
            //there is no subject, just load null
            state.EmitLoadNullAsPValue();
            return;
        }

        //We don't actually need static arguments, just emit the corresponding opcodes
        foreach (var compileTimeValue in staticArgv)
            compileTimeValue.EmitLoadAsPValue(state);

        //pack arguments (including static ones) into the argv array, but exclude subject (the first argument)
        state.FillArgv(argc);
        state.ReadArgv(argc);

        //call constructor of FunctionalPartialCall

        state.Il.Emit(OpCodes.Newobj, partialCallCtor);

        //wrap in PValue
        if (ins.JustEffect)
        {
            state.Il.Emit(OpCodes.Pop);
        }
        else
        {
            state.EmitStoreTemp(0);
            state.EmitLoadLocal(state.SctxLocal);
            state.EmitLoadTemp(0);
            state.EmitVirtualCall(Compiler.Cil.Compiler.CreateNativePValue);
        }
    }
}
