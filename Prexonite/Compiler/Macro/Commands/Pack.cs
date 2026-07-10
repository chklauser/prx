using Prexonite.Modular;
using Prexonite.Properties;

namespace Prexonite.Compiler.Macro.Commands;

public class Pack : MacroCommand
{
    public const string Alias = @"macro\pack";

    #region Singleton pattern

    public static Pack Instance { get; } = new();

    Pack()
        : base(Alias) { }

    #endregion

    #region Overrides of MacroCommand

    protected override void DoExpand(MacroContext context)
    {
        if (context.Invocation.Arguments.Count < 1)
        {
            context.ReportMessage(
                Message.Error(
                    string.Format(Resources.Pack_Usage_obj_missing, Alias),
                    context.Invocation.Position,
                    MessageClasses.PackUsage
                )
            );
            return;
        }

        context.EstablishMacroContext();

        // [| context.StoreForTransport(boxed($arg0)) |]

        var getContext = context.CreateIndirectCall(
            context.CreateCall(EntityRef.Variable.Local.Create(MacroAliases.ContextAlias))
        );
        var boxedArg0 = context.CreateCall(
            EntityRef.Command.Create(Engine.BoxedAlias),
            PCall.Get,
            context.Invocation.Arguments[0]
        );
        context.Block.Expression = context.CreateGetSetMember(
            getContext,
            PCall.Get,
            "StoreForTransport",
            boxedArg0
        );
    }

    #endregion
}
