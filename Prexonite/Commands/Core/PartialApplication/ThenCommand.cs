

using System.Diagnostics;
using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.Core.PartialApplication;

public class ThenCommand : PCommand, ICilCompilerAware
{
    #region Singleton pattern

    ThenCommand()
    {
    }

    public static ThenCommand Instance { get; } = new();

    #endregion

    #region Overrides of PCommand

    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args)
    {
        return RunStatically(sctx, args.ToArray());
    }

    public static PValue RunStatically(StackContext sctx, PValue[] args)
    {
        if (args.Length < 2)
            throw new PrexoniteException("then command requires two arguments.");

        return sctx.CreateNativePValue(new CallComposition(args[0], args[1]));
    }

    #endregion

    #region Implementation of ICilCompilerAware

    public CompilationFlags CheckQualification(Instruction ins)
    {
        return CompilationFlags.PrefersRunStatically;
    }

    public void ImplementInCil(CompilerState state, Instruction ins)
    {
        throw new NotSupportedException();
    }

    #endregion
}

public class CallComposition : IIndirectCall
{
    public PValue InnerExpression { [DebuggerStepThrough] get; }

    public PValue OuterExpression { [DebuggerStepThrough] get; }

    public CallComposition(PValue innerExpression, PValue outerExpression)
    {
        InnerExpression = innerExpression ?? throw new ArgumentNullException(nameof(innerExpression));
        OuterExpression = outerExpression ?? throw new ArgumentNullException(nameof(outerExpression));
    }

    #region Implementation of IIndirectCall

    public PValue IndirectCall(StackContext sctx, params ReadOnlySpan<PValue> args)
    {
        return OuterExpression.IndirectCall(sctx, InnerExpression.IndirectCall(sctx, args));
    }

    #endregion

    public override string ToString()
    {
        return $"{InnerExpression} then ({OuterExpression})";
    }
}