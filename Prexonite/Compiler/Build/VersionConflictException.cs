

using Prexonite.Modular;

namespace Prexonite.Compiler.Build;

sealed class VersionConflictException : Exception
{
    public VersionConflictException(ModuleName existingModule, ModuleName newModule, ModuleName offendingModule)
    {
        ExistingModule = existingModule;
        NewModule = newModule;
        OffendingModule = offendingModule;
        Data[nameof(ExistingModule)] = existingModule;
        Data[nameof(NewModule)] = newModule;
        Data[nameof(OffendingModule)] = offendingModule;
    }

    public ModuleName ExistingModule { get; }

    public ModuleName NewModule { get; }

    public ModuleName OffendingModule { get; }

    public override string Message =>
        $"Version conflict detected in dependencies of module {OffendingModule}, concerning module {ExistingModule.Id}. Existing version {ExistingModule.Version}, new version {NewModule.Version}.";
}