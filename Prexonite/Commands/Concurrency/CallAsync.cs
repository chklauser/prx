

using System.Diagnostics;
using Prexonite.Commands.Core;
using Prexonite.Compiler.Cil;
using Prexonite.Compiler.Macro.Commands;
using Prexonite.Concurrency;
using Prexonite.Modular;

namespace Prexonite.Commands.Concurrency;

public class CallAsync : PCommand, ICilCompilerAware
{
    #region Singleton pattern

    CallAsync()
    {
    }

    public static CallAsync Instance { get; } = new();

    #endregion

    public const string Alias = @"call\async\perform";

    #region Overrides of PCommand

    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args)
    {
        return RunStatically(sctx, args.ToArray());
    }

    public static PValue RunStatically(StackContext sctx, PValue[]? args)
    {
        if (sctx == null)
            throw new ArgumentNullException(nameof(sctx));
        if (args == null || args.Length == 0 || args[0] == null)
            return PType.Null.CreatePValue();

        var iargs = Call.FlattenArguments(sctx, args, 1);

        var retChan = new Channel();
        var T = new Thread(() =>
        {
            PValue result;
            try
            {
                result = args[0].IndirectCall(sctx, [..iargs]);
            }
            catch (Exception ex)
            {
                result = sctx.CreateNativePValue(ex);
            }
            retChan.Send(result);
        })
        {
            IsBackground = true,
        };
        T.Start();
        return PType.Object.CreatePValue(retChan);
    }

    public static Channel RunAsync(StackContext sctx, Func<PValue> comp)
    {
        var retChan = new Channel();
        var T = new Thread(() => retChan.Send(comp()))
        {
            IsBackground = true,
        };
        T.Start();
        return retChan;
    }

    #endregion

    #region Implementation of ICilCompilerAware

    CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
    {
        return CompilationFlags.PrefersRunStatically;
    }

    void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
    {
        throw new NotSupportedException("The command " + GetType().Name +
            " does not support CIL compilation via ICilCompilerAware.");
    }

    #endregion

    #region Partial application via call\star

    public PartialCallWrapper Partial { [DebuggerStepThrough] get; } = new(
        Engine.Call_AsyncAlias, EntityRef.Command.Create(Alias));

    #endregion
}