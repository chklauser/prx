using Prexonite.Compiler;

namespace Prexonite.Commands.Core;

/// <summary>
///     A command that aids in generating debug output. Best used in conjunction with the <see cref = "DebugHook" />.
/// </summary>
public class Debug : PCommand
{
    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args)
    {
        if (sctx == null)
            throw new ArgumentNullException(nameof(sctx));
        if (sctx is not FunctionContext fctx)
            return false;
        var debugging = DebugHook.IsDebuggingEnabled(fctx.Implementation);
        var println = sctx.ParentEngine.Commands[Engine.PrintLineAlias];
        if (debugging)
            foreach (var arg in args)
            {
                println.Run(sctx, string.Concat("DEBUG ??? = ", arg.CallToString(sctx)));
            }
        return debugging;
    }
}
