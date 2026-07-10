using Prexonite.Compiler.Cil;
using Prexonite.Modular;

namespace Prexonite.Commands.Core;

public class CreateModuleName : PCommand, ICilCompilerAware
{
    #region Singleton pattern

    public static CreateModuleName Instance { get; } = new();

    CreateModuleName() { }

    #endregion

    public const string Alias = "create_module_name";

    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args)
    {
        return RunStatically(sctx, args.ToArray());
    }

    public static PValue RunStatically(StackContext sctx, PValue[] args)
    {
        if (args.Length < 1)
            throw new PrexoniteException(Alias + "(...) requires at least one argument.");

        PValue rawVersion;

        if (args.Length == 1)
        {
            if (args[0].Type == PType.Object[typeof(MetaEntry)])
            {
                var entry = (MetaEntry)args[0].Value!;
                if (ModuleName.TryParse(entry, out var moduleName))
                    return sctx.CreateNativePValue(sctx.Cache[moduleName]);
                else
                    return PType.Null;
            }
            else
            {
                var raw = args[0].CallToString(sctx);

                if (ModuleName.TryParse(raw, out var moduleName))
                    return sctx.CreateNativePValue(sctx.Cache[moduleName]);
                else
                    return PType.Null;
            }
        }
        else if ((rawVersion = args[1]).Type.Equals(PType.Object[typeof(Version)]))
        {
            var raw = args[0].CallToString(sctx);

            return sctx.CreateNativePValue(
                sctx.Cache[new ModuleName(raw, (Version)rawVersion.Value!)]
            );
        }
        else
        {
            var raw = args[0].CallToString(sctx);

            if (Version.TryParse(rawVersion.CallToString(sctx), out var version))
                return sctx.CreateNativePValue(sctx.Cache[new ModuleName(raw, version)]);
            else
                return PType.Null;
        }
    }

    public CompilationFlags CheckQualification(Instruction ins)
    {
        return CompilationFlags.PrefersRunStatically;
    }

    public void ImplementInCil(CompilerState state, Instruction ins)
    {
        throw new NotSupportedException(
            "The command " + Alias + " does provide a custom cil implementation. "
        );
    }
}
