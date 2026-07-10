namespace Prexonite.Commands.Core.PartialApplication;

public class PartialCallStarImplCommand : PartialApplicationCommandBase<object>
{
    #region Singleton pattern

    public static PartialCallStarImplCommand Instance { get; } = new();

    PartialCallStarImplCommand() { }

    #endregion

    public const string Alias = @"pa\call\star";

    #region Overrides of PartialApplicationCommandBase<object>

    protected override IIndirectCall CreatePartialApplication(
        StackContext sctx,
        int[] mappings,
        PValue[] closedArguments,
        object parameter
    )
    {
        return new PartialCallStar(mappings, closedArguments);
    }

    protected override Type GetPartialCallRepresentationType(object parameter)
    {
        return typeof(PartialCallStar);
    }

    #endregion
}
