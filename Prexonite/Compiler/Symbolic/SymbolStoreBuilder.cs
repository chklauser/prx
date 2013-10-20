// Prexonite
// 
// Copyright (c) 2013, Christian Klauser
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, 
//  are permitted provided that the following conditions are met:
// 
//     Redistributions of source code must retain the above copyright notice, 
//          this list of conditions and the following disclaimer.
//     Redistributions in binary form must reproduce the above copyright notice, 
//          this list of conditions and the following disclaimer in the 
//          documentation and/or other materials provided with the distribution.
//     The names of the contributors may be used to endorse or 
//          promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
//  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
//  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
//  IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
//  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
//  (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
//  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
//  WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING 
//  IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Annotations;

namespace Prexonite.Compiler.Symbolic
{
    public abstract class SymbolStoreBuilder
    {
        public abstract void Forward(SymbolOrigin sourceDescription, [NotNull] ISymbolView<Symbol> source, [NotNull] IEnumerable<SymbolTransferDirective> directives);
        public abstract SymbolStore ToSymbolStore();

        [NotNull]
        public static SymbolStoreBuilder Create(ISymbolView<Symbol> existingNamespace)
        {
            return new Impl(existingNamespace);
        }

        [NotNull]
        public static SymbolStoreBuilder Create()
        {
            return new Impl();
        }

        #region Internal data structures

        private sealed class ImportStatement : List<SymbolTransferDirective>
        {
            [NotNull] private readonly ISymbolView<Symbol> _source;
            [NotNull] private readonly SymbolOrigin _origin;

            [NotNull]
            public ISymbolView<Symbol> Source
            {
                get { return _source; }
            }

            [NotNull]
            public SymbolOrigin Origin
            {
                get { return _origin; }
            }

            public ImportStatement([NotNull] ISymbolView<Symbol> source, [NotNull] SymbolOrigin origin)
            {
                if (source == null) throw new ArgumentNullException("source");
                if (origin == null) throw new ArgumentNullException("origin");
                _source = source;
                _origin = origin;
            }
        }

        private sealed class ImportStatementSet :
            KeyedCollection<SymbolOrigin, ImportStatement>
        {
            protected override SymbolOrigin GetKeyForItem(ImportStatement item)
            {
                return item.Origin;
            }

            [ContractAnnotation("=>true,statement:notnull;=>false,statement:null")]
            public bool TryGet(SymbolOrigin origin, out ImportStatement statement)
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

        private class Impl : SymbolStoreBuilder
        {
            [CanBeNull] private readonly ISymbolView<Symbol> _existingNamespace;

            [NotNull] private readonly ImportStatementSet _statements = new ImportStatementSet();

            public Impl([CanBeNull] ISymbolView<Symbol> existingNamespace = null)
            {
                _existingNamespace = existingNamespace;
            }

            public override void Forward(SymbolOrigin sourceDescription, ISymbolView<Symbol> source,
                IEnumerable<SymbolTransferDirective> directives)
            {
                ImportStatement statement;
                if (!_statements.TryGet(sourceDescription, out statement))
                    _statements.Add(statement = new ImportStatement(source, sourceDescription));

                statement.AddRange(directives);
            }

            public override SymbolStore ToSymbolStore()
            {
                return SymbolStore.Create(_existingNamespace, _statements.SelectMany(_applyDirectives));
            }

            private IEnumerable<SymbolInfo> _applyDirectives(ImportStatement import)
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
                                List<string> destinations;
                                if (!renames.TryGetValue(r.OriginalName, out destinations))
                                    renames.Add(r.OriginalName, destinations = new List<string>());
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

            private IEnumerable<SymbolInfo> _applyDirectivesSelective(ImportStatement import,
                ISymbolView<Symbol> symbolSource)
            {
                return import.SelectMaybe(SymbolTransferDirective.Matching(() => null, rename =>
                    {
                        Symbol symbol;
                        var originalName = rename.OriginalName;
                        if (!symbolSource.TryGet(originalName, out symbol))
                        {
                            symbol = _createSymbolNotFoundSymbol(import, originalName);
                        }

                        return _createSymbolInfo(import, rename.NewName, symbol);
                        // ReSharper disable ImplicitlyCapturedClosure
                    }, drop => null
                    ));
                // ReSharper restore ImplicitlyCapturedClosure
            }

            [NotNull]
            private static IEnumerable<SymbolInfo> _applyDirectivesWildcard(ImportStatement import,
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
                            List<string> destinationNames;
                            if (renames.TryGetValue(kvp.Key, out destinationNames))
                                return
                                    destinationNames.Select(d => _createSymbolInfo(import, d, kvp.Value));
                            else
                                return _createSymbolInfo(import, kvp.Key, kvp.Value).Singleton();
                        })
                    .Append(_missingErrorSymbols(import, renames, mentioned));
            }

            private static IEnumerable<SymbolInfo> _missingErrorSymbols(ImportStatement import,
                Dictionary<string, List<string>> renames, HashSet<string> mentioned)
            {
                return renames
                    .Where(kvp => !mentioned.Contains(kvp.Key))
                    .SelectMany(kvp => kvp.Value
                        .Select(dest => _createSymbolInfo(import, dest, _createSymbolNotFoundSymbol(import, kvp.Key))));
            }

            private static Symbol _createSymbolNotFoundSymbol(ImportStatement import, string n)
            {
                var offendingDirective = _findOffendingDirective(n, import);
                return SymbolStore._CreateSymbolNotFoundError(n, offendingDirective.Position);
            }

            private static SymbolTransferDirective _findOffendingDirective(string name, ImportStatement import)
            {
                return import.Find(directive => _matchingName(name, directive));
            }

            private static bool _matchingName(string name, SymbolTransferDirective directive)
            {
                return directive.Match(() => false, r => Engine.StringsAreEqual(name, r.OriginalName),
                    d => Engine.StringsAreEqual(name, d.Name));
            }

            private static SymbolInfo _createSymbolInfo(ImportStatement import, string name, Symbol symbol)
            {
                return new SymbolInfo(symbol, import.Origin, name);
            }
        }

        #endregion

    }
}