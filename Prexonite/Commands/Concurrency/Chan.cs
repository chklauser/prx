using System.Reflection;
using System.Reflection.Emit;
using Prexonite.Compiler.Cil;
using Prexonite.Concurrency;

namespace Prexonite.Commands.Concurrency;

public class Chan : PCommand, ICilCompilerAware
{
    #region Singleton pattern

    Chan() { }

    public static Chan Instance { get; } = new();

    #endregion

    #region Overrides of PCommand

    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args)
    {
        return PType.Object.CreatePValue(new Channel());
    }

    #endregion

    #region Implementation of ICilCompilerAware

    CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
    {
        return CompilationFlags.PrefersCustomImplementation;
    }

    static readonly ConstructorInfo _channelCtor = typeof(Channel).GetConstructor([])!;

    static readonly ConstructorInfo _newPValue = typeof(PValue).GetConstructor([
        typeof(object),
        typeof(PType),
    ])!;

    void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
    {
        state.EmitIgnoreArguments(ins.Arguments);
        state.Il.Emit(OpCodes.Newobj, _channelCtor);
        PType.PrexoniteObjectTypeProxy._ImplementInCil(state, typeof(Channel));
        state.Il.Emit(OpCodes.Newobj, _newPValue);
    }

    #endregion
}
