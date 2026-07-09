

using Prexonite.Modular;

namespace Prexonite.Compiler.Macro.Commands;

public class CallSub : MacroCommand
{
    public const string Alias = @"call\sub";

    #region Singleton pattern

    public static CallSub Instance { get; } = new();

    CallSub() : base(Alias)
    {
    }

    #endregion

    #region Overrides of MacroCommand

    protected override void DoExpand(MacroContext context)
    {
        var perform =
            context.Factory.Call(context.Invocation.Position, EntityRef.Command.Create(Engine.CallSubPerformAlias),
                PCall.Get, context.Invocation.Arguments.ToArray());
        var interpret = context.Factory.Expand(context.Invocation.Position,
            EntityRef.MacroCommand.Create(CallSubInterpret.Alias), context.Invocation.Call);
            
        interpret.Arguments.Add(perform);

        context.Block.Expression = interpret;
    }

    #endregion
}