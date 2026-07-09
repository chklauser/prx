

using System.Reflection;

namespace Prexonite.Commands.Core.PartialApplication;

public class PartialConstructionCommand : PartialWithPTypeCommandBase<RuntimePTypeInfo,CompileTimePTypeInfo>
{
    #region Singleton pattern

    PartialConstructionCommand()
    {
    }

    ConstructorInfo? _ptypeConstructCtor;

    public static PartialConstructionCommand Instance { get; } = new();

    #endregion

    #region Overrides of PartialApplicationCommandBase<TypeInfo>

    protected override IIndirectCall CreatePartialApplication(StackContext sctx, int[] mappings,
        PValue[] closedArguments, RuntimePTypeInfo parameter)
    {
        return new PartialConstruction(mappings, closedArguments, parameter.Type);
    }

    protected override ConstructorInfo GetConstructorCtor(CompileTimePTypeInfo parameter)
    {
        var ty = GetPartialCallRepresentationType(parameter);
        return _ptypeConstructCtor ??= ty.GetConstructor(
                [typeof (int[]), typeof (PValue[]), typeof (PType)])
            ?? throw new InvalidOperationException($"{ty} does not have an (int[], PValue[], PValue) constructor.");
    }

    protected override Type GetPartialCallRepresentationType(CompileTimePTypeInfo parameter)
    {
        return typeof (PartialConstruction);
    }

    protected override string PartialApplicationKind => "Partial Construction";

    #endregion
}