using System.Reflection;

namespace Prexonite.Commands.Core.PartialApplication;

public class PartialTypeCheckCommand
    : PartialWithPTypeCommandBase<RuntimePTypeInfo, CompileTimePTypeInfo>
{
    PartialTypeCheckCommand() { }

    public static PartialTypeCheckCommand Instance { get; } = new();

    ConstructorInfo? _partialTypeCheckCtor;

    protected override IIndirectCall CreatePartialApplication(
        StackContext sctx,
        int[] mappings,
        PValue[] closedArguments,
        RuntimePTypeInfo parameter
    )
    {
        return new PartialTypeCheck(mappings, closedArguments, parameter.Type);
    }

    protected override ConstructorInfo GetConstructorCtor(CompileTimePTypeInfo parameter)
    {
        return _partialTypeCheckCtor ??=
            typeof(PartialTypeCheck).GetConstructor([
                typeof(int[]),
                typeof(PValue[]),
                typeof(PType),
            ])
            ?? throw new InvalidOperationException(
                $"{nameof(PartialTypeCheck)} does not have an (int[], PValue[], PType) constructor."
            );
    }

    protected override Type GetPartialCallRepresentationType(CompileTimePTypeInfo parameter)
    {
        return typeof(PartialTypeCheck);
    }

    protected override string PartialApplicationKind => "Partial type check";
}
