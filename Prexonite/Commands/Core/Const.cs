using System.Reflection;
using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.Core;

public class Const : PCommand, ICilCompilerAware
{
    #region Singleton pattern

    public static Const Instance { get; } = new();

    Const() { }

    #endregion

    public const string Alias = "const";

    public static PValue RunStatically(StackContext sctx, PValue[] args)
    {
        PValue constant;
        if (args.Length < 1)
            constant = PType.Null;
        else
            constant = args[0];

        return CreateConstFunction(constant, sctx);
    }

    class Impl : IIndirectCall
    {
        readonly PValue _value;

        public Impl(PValue value)
        {
            _value = value;
        }

        public PValue IndirectCall(StackContext sctx, params ReadOnlySpan<PValue> args)
        {
            return _value;
        }
    }

    MethodInfo? _createConstFunctionInfoCache;

    MethodInfo createConstFunction
    {
        get
        {
            return _createConstFunctionInfoCache ??= typeof(Const).GetMethod(
                nameof(CreateConstFunction),
                [typeof(PValue), typeof(StackContext)]
            )!;
        }
    }

    public static PValue CreateConstFunction(PValue constant, StackContext sctx)
    {
        return sctx.CreateNativePValue(new Impl(constant));
    }

    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args)
    {
        return RunStatically(sctx, args.ToArray());
    }

    public CompilationFlags CheckQualification(Instruction ins)
    {
        return CompilationFlags.PrefersCustomImplementation;
    }

    public void ImplementInCil(CompilerState state, Instruction ins)
    {
        var argc = ins.Arguments;
        if (argc > 1)
            state.EmitIgnoreArguments(argc - 1);

        state.EmitLoadLocal(state.SctxLocal);
        if (argc == 0)
            state.EmitLoadNullAsPValue();

        state.EmitCall(createConstFunction);
    }
}
