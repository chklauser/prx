

using System.Collections.ObjectModel;
using JetBrains.Annotations;

namespace Prexonite.Compiler.Symbolic;

public abstract class SymbolStoreBuilder
{
    public abstract void Forward(SymbolOrigin sourceDescription, ISymbolView<Symbol> source, IEnumerable<SymbolTransferDirective> directives);
    public abstract SymbolStore ToSymbolStore();

    public static SymbolStoreBuilder Create(ISymbolView<Symbol>? existingNamespace)
    {
        return new Impl{ ExistingNamespace = existingNamespace };
    }

    public static SymbolStoreBuilder Create()
    {
        return new Impl();
    }
        
    public abstract ISymbolView<Symbol>? ExistingNamespace { get; set; }

    #region Internal data structures

    sealed class ImportStatement : List<SymbolTransferDirective>
    {
        public ISymbolView<Symbol> Source { get; }

        public SymbolOrigin Origin { get; }

        public ImportStatement(ISymbolView<Symbol> source, SymbolOrigin origin)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Origin = origin ?? throw new ArgumentNullException(nameof(origin));
        }
    }

    sealed class ImportStatementSet :
        KeyedCollection<SymbolOrigin, ImportStatement>
    {
        protected override SymbolOrigin GetKeyForItem(ImportStatement item)
        {
            return item.Origin;
        }

        [ContractAnnotation("=>true,statement:notnull;=>false,statement:null")]
        public bool TryGet(SymbolOrigin origin, [NotNullWhen(true)] out ImportStatement? statement)
        {
            if (Contains(origin))
            {
                statement = this[origin];
                return true;
            }
            else
            {
                statement = null;
                return false;
            }
        }
    }

    #endregion

    #region Implementation

    class Impl : SymbolStoreBuilder
    {
        public override ISymbolView<Symbol>? ExistingNamespace { get; set; }

        readonly ImportStatementSet _statements = new();

        public override void Forward(SymbolOrigin sourceDescription, ISymbolView<Symbol> source,
            IEnumerable<SymbolTransferDirective> directives)
        {
            if (!_statements.TryGet(sourceDescription, out var statement))
                _statements.Add(statement = new(source, sourceDescription));

            statement.AddRange(directives);
        }

        public override SymbolStore ToSymbolStore()
        {
            return SymbolStore.Create(ExistingNamespace, _statements.SelectMany(_applyDirectives));
        }

        IEnumerable<SymbolInfo> _applyDirectives(ImportStatement import)
        {
            // Determine whether we are performing a wildcard or a selective import
            var isWildcard = false;
            var drops = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var renames = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            // Convert directives into a pair of indexes (drops and renames) 
            // for efficient application
            foreach (var directive in import)
            {
                // ReSharper disable RedundantArgumentName
                directive.Match(
                    onWildcard: () => { isWildcard = true; },
                    onRename: r =>
                    {
                        if (!renames.TryGetValue(r.OriginalName, out var destinations))
                            renames.Add(r.OriginalName, destinations = new());
                        destinations.Add(r.NewName);
                    },
                    onDrop: d => drops.Add(d.Name)
                );
                // ReSharper restore RedundantArgumentName
            }

            // Apply the directives either to the entire source namespace (wildcard) or selectively
            return isWildcard
                ? _applyDirectivesWildcard(import, import.Source, drops, renames)
                : _applyDirectivesSelective(import, import.Source);
        }

        IEnumerable<SymbolInfo> _applyDirectivesSelective(ImportStatement import,
            ISymbolView<Symbol> symbolSource)
        {
            return import.SelectMaybe(SymbolTransferDirective.Matching<SymbolInfo?>(() => null, rename =>
                {
                    var originalName = rename.OriginalName;
                    if (!symbolSource.TryGet(originalName, out var symbol))
                    {
                        symbol = _createSymbolNotFoundSymbol(import, originalName);
                    }

                    return _createSymbolInfo(import, rename.NewName, symbol);
                    // ReSharper disable ImplicitlyCapturedClosure
                }, _ => null
            ));
            // ReSharper restore ImplicitlyCapturedClosure
        }

        static IEnumerable<SymbolInfo> _applyDirectivesWildcard(ImportStatement import,
            IEnumerable<KeyValuePair<string, Symbol>> symbolSource,
            HashSet<string> drops, Dictionary<string, List<string>> renames)
        {
            var mentioned = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            return symbolSource
                // ReSharper disable ImplicitlyCapturedClosure
                .Where(kvp => // ReSharper restore ImplicitlyCapturedClosure
                {
                    mentioned.Add(kvp.Key);
                    return !drops.Contains(kvp.Key);
                })
                // ReSharper disable ImplicitlyCapturedClosure
                .SelectMany(kvp => // ReSharper restore ImplicitlyCapturedClosure
                {
                    var (key, value) = kvp;
                    if (renames.TryGetValue(key, out var destinationNames))
                        return destinationNames.Select(d => _createSymbolInfo(import, d, value));
                    else
                        return _createSymbolInfo(import, key, value).Singleton();
                })
                .Append(_missingErrorSymbols(import, renames, mentioned));
        }

        static IEnumerable<SymbolInfo> _missingErrorSymbols(ImportStatement import,
            Dictionary<string, List<string>> renames, HashSet<string> mentioned)
        {
            return renames
                .Where(kvp => !mentioned.Contains(kvp.Key))
                .SelectMany(kvp => kvp.Value
                    .Select(dest => _createSymbolInfo(import, dest, _createSymbolNotFoundSymbol(import, kvp.Key))));
        }

        static Symbol _createSymbolNotFoundSymbol(ImportStatement import, string n)
        {
            var offendingDirective = _findOffendingDirective(n, import);
            return SymbolStore._CreateSymbolNotFoundError(n, offendingDirective.Position);
        }

        static SymbolTransferDirective _findOffendingDirective(string name, ImportStatement import)
        {
            // We should always find a matching name because it must have triggered this error path.
            return import.Find(directive => _matchingName(name, directive))!;
        }

        static bool _matchingName(string name, SymbolTransferDirective directive)
        {
            return directive.Match(() => false, r => Engine.StringsAreEqual(name, r.OriginalName),
                d => Engine.StringsAreEqual(name, d.Name));
        }

        static SymbolInfo _createSymbolInfo(ImportStatement import, string name, Symbol symbol)
        {
            return new(symbol, import.Origin, name);
        }
    }

    #endregion

}