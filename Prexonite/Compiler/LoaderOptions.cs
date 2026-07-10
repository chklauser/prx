using System.Diagnostics;
using JetBrains.Annotations;
using Prexonite.Compiler.Symbolic;

namespace Prexonite.Compiler;

[DebuggerStepThrough]
public class LoaderOptions
{
    #region Construction

    public LoaderOptions(Engine? parentEngine, Application? targetApplication)
    {
        ParentEngine = parentEngine;
        TargetApplication = targetApplication;
        ExternalSymbols = new EmptySymbolView<Symbol>();
    }

    public LoaderOptions(
        Engine parentEngine,
        Application targetApplication,
        ISymbolView<Symbol>? externalSymbols
    )
    {
        ParentEngine = parentEngine ?? throw new ArgumentNullException(nameof(parentEngine));
        TargetApplication =
            targetApplication ?? throw new ArgumentNullException(nameof(targetApplication));
        ExternalSymbols =
            externalSymbols ?? throw new ArgumentNullException(nameof(externalSymbols));
    }

    #endregion

    #region Properties

    public Engine? ParentEngine { get; }

    public Application? TargetApplication { get; }

    public ISymbolView<Symbol> ExternalSymbols { get; }

    bool? _registerCommands;
    public bool RegisterCommands
    {
        get => _registerCommands ?? true;
        set => _registerCommands = value;
    }

    bool? _reconstructSymbols;
    public bool ReconstructSymbols
    {
        get => _reconstructSymbols ?? true;
        set => _reconstructSymbols = value;
    }

    bool? _storeSymbols;
    public bool StoreSymbols
    {
        get => _storeSymbols ?? true;
        set => _storeSymbols = value;
    }

    bool? _dumpExternalSymbols;

    /// <summary>
    /// Indicates whether the loader will include external symbols when storing a representation of the application.
    /// </summary>
    /// <para>
    /// This is only useful to diagnose the symbol environment that the loader is working with. While the resulting
    /// image can be loaded it should not be used in production code as it effectively re-exports all of the symbols
    /// of all of its dependencies (including conflicts).
    /// </para>
    public bool DumpExternalSymbols
    {
        get => _dumpExternalSymbols ?? false;
        set => _dumpExternalSymbols = value;
    }

    bool? _useIndicesLocally;
    public bool UseIndicesLocally
    {
        get => _useIndicesLocally ?? true;
        set => _useIndicesLocally = value;
    }

    bool? _storeSourceInformation;
    public bool StoreSourceInformation
    {
        get => _storeSourceInformation ?? false;
        set => _storeSourceInformation = value;
    }

    bool? _preflightModeEnabled;

    /// <summary>
    /// Preflight mode causes the parser to abort at the
    /// first non-meta construct, giving the user the opportunity
    /// to inspect a file's "header" without fully compiling
    /// that file.
    /// </summary>
    public bool PreflightModeEnabled
    {
        get => _preflightModeEnabled ?? false;
        set => _preflightModeEnabled = value;
    }

    bool? _flagLiteralsEnabled;

    ///<summary>
    /// Determines whether flag literals (-f, --query, --option=value) are parsed globally. Not backwards compatible
    /// because of overlap with unary minus and pre-decrement.
    ///</summary>
    [PublicAPI]
    public bool FlagLiteralsEnabled
    {
        get => _flagLiteralsEnabled ?? false;
        set => _flagLiteralsEnabled = value;
    }

    string? _storeNewLine;

    /// <summary>
    /// The line separator to use when storing a compiled Prexonite program.
    /// </summary>
    /// <see cref="Loader.Store(System.Text.StringBuilder)"/>
    [PublicAPI]
    public string StoreNewLine
    {
        get => _storeNewLine ?? "\n";
        set => _storeNewLine = value;
    }

    /// <summary>
    /// If set, then `add` and `require` (transclusion) is only allowed once for each file. This ensures that
    /// the order in which code is loaded into the module is deterministic.
    /// </summary>
    [PublicAPI]
    public bool? EnforceDeterministicCodeOrder { get; set; }

    #endregion

    public void InheritFrom(LoaderOptions options)
    {
        if (options == null)
            throw new ArgumentNullException(nameof(options));

        _registerCommands ??= options._registerCommands;
        _reconstructSymbols ??= options._reconstructSymbols;
        _storeSymbols ??= options._storeSymbols;
        _dumpExternalSymbols ??= options._dumpExternalSymbols;
        _useIndicesLocally ??= options._useIndicesLocally;
        _storeSourceInformation ??= options._storeSourceInformation;
        _preflightModeEnabled ??= options._preflightModeEnabled;
        _flagLiteralsEnabled ??= options._flagLiteralsEnabled;
        _storeNewLine ??= options._storeNewLine;
        EnforceDeterministicCodeOrder ??= options.EnforceDeterministicCodeOrder;
    }
}
