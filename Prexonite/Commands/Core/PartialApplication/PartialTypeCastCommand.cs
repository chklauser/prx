using System.Reflection;

namespace Prexonite.Commands.Core.PartialApplication;

public class PartialTypecastCommand
    : PartialWithPTypeCommandBase<RuntimePTypeInfo, CompileTimePTypeInfo>
{
    PartialTypecastCommand() { }

    public static PartialTypecastCommand Instance { get; } = new();

    ConstructorInfo? _partialTypeCastCtor;

    protected override IIndirectCall CreatePartialApplication(
        StackContext sctx,
        int[] mappings,
        PValue[] closedArguments,
        RuntimePTypeInfo parameter
    )
    {
        return new PartialTypecast(mappings, closedArguments, parameter.Type);
    }

    protected override ConstructorInfo GetConstructorCtor(CompileTimePTypeInfo parameter)
    {
        return _partialTypeCastCtor ??=
            typeof(PartialTypecast).GetConstructor([typeof(int[]), typeof(PValue[]), typeof(PType)])
            ?? throw new InvalidOperationException(
                $"{nameof(PartialTypecast)} does not have an (int[], PValue[], PType) constructor."
            );
    }

    protected override Type GetPartialCallRepresentationType(CompileTimePTypeInfo parameter)
    {
        return typeof(PartialTypecast);
    }

    protected override string PartialApplicationKind => "Partial type cast";
}
