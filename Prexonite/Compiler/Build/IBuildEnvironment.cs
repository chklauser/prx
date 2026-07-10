using Prexonite.Compiler.Symbolic;
using Prexonite.Modular;

namespace Prexonite.Compiler.Build;

public interface IBuildEnvironment : IDisposable
{
    SymbolStore ExternalSymbols { get; }

    Module Module { get; }

    Application InstantiateForBuild();

    /// <summary>
    /// Creates the loader instance for this target.
    /// </summary>
    /// <param name="defaults">The default values for the loader options.</param>
    /// <param name="compilationTarget">The module instance to be used for compiling the module.</param>
    /// <returns></returns>
    Loader CreateLoader(LoaderOptions? defaults = null, Application? compilationTarget = null);
}
