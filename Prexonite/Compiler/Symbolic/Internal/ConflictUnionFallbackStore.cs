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
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Prexonite.Properties;

namespace Prexonite.Compiler.Symbolic.Internal
{
    internal class ConflictUnionFallbackStore : SymbolStore
    {
        private readonly ISymbolView<Symbol> _parent;
        private readonly SymbolTable<Symbol> _union;
        private SymbolTable<Symbol> _local;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        internal ConflictUnionFallbackStore(ISymbolView<Symbol> parent = null, IEnumerable<SymbolInfo> conflictUnionSource = null)
        {
            _parent = parent;

            if (conflictUnionSource != null)
            {
                var u = new SymbolTable<Symbol>();
                u.AddRange(conflictUnionSource.GroupBy(i => i.Name, Engine.DefaultStringComparer).Select(_unifySymbols));
                if (u.Count > 0)
                    _union = u;
            }
        }

        private static KeyValuePair<string, Symbol> _unifySymbols(IGrouping<string, SymbolInfo> source)
        {
            using (var e = source.GetEnumerator())
            {
                if (!e.MoveNext())
                    // ReSharper disable NotResolvedInText
                    throw new ArgumentOutOfRangeException("conflictUnionSource", source.Key, Resources.ConflictUnionFallbackStore__unifySymbols_Invalid_key_in_source_for_symbol_store_);
                // ReSharper restore NotResolvedInText

                var unionInfo = e.Current;
                var x = unionInfo;

                while (e.MoveNext())
                {
                    var y = e.Current;
                    var merged = _merge(x, y);
                    if (merged == null)
                        return _unifySymbolsDualMode(new SymbolInfo(x.Symbol, x.Origin, unionInfo.Name), y, e);
                    else
                        x = new SymbolInfo(merged,SymbolOrigin.MergedScope.CreateMerged(x.Origin,y.Origin),x.Name);
                }

                return new KeyValuePair<string, Symbol>(unionInfo.Name, x.Symbol);
            }
        }

        private static KeyValuePair<string, Symbol> _unifySymbolsDualMode(SymbolInfo first, SymbolInfo second, IEnumerator<SymbolInfo> e)
        {
            var x1 = first;
            var x2 = second;

            while (e.MoveNext())
            {
                var y = e.Current;

                var merged = _merge(x1, y);
                if (merged != null)
                {
                    x1 = new SymbolInfo(merged,SymbolOrigin.MergedScope.CreateMerged(x1.Origin,y.Origin),x1.Name);
                }
                else
                {
                    merged = _merge(x2,y);
                    if (merged != null)
                    {
                        x2 = new SymbolInfo(merged,SymbolOrigin.MergedScope.CreateMerged(x2.Origin,y.Origin),x2.Name);
                    }
                    else
                    {
                        return _unifySymbolsMultiMode(x1, x2, y, e);
                    }
                }
            }

            var msg = string.Format("There are two incompatible declarations of the symbol {0} in this scope. " +
                                    "One comes from {1}, the other one from {2}.", first.Name, first.Origin,
                                    second.Origin);

            return new KeyValuePair<string, Symbol>(first.Name,
                                                    Symbol.CreateMessage(Message.Create(MessageSeverity.Error,
                                                                                msg,
                                                                                first.Origin.Position,
                                                                                MessageClasses.SymbolConflict), x1.Symbol));
        }

        private static KeyValuePair<string, Symbol> _unifySymbolsMultiMode(SymbolInfo first, SymbolInfo second, SymbolInfo third, IEnumerator<SymbolInfo> e)
        {
            var symbols = new List<SymbolInfo> { first, second, third };
            var xs = new List<SymbolInfo> { first, second, third };

            while (e.MoveNext())
            {
                var y = e.Current;
                int i;
                for (i = 0; i < xs.Count; i++)
                {
                    var thisSymbol = xs[i];
                    var merged = _merge(thisSymbol, y);
                    if (merged != null)
                    {
                        xs[i] = new SymbolInfo(merged,SymbolOrigin.MergedScope.CreateMerged(thisSymbol.Origin,y.Origin),thisSymbol.Name);
                        break;
                    }
                }

                if (i == xs.Count) // did not unify with any of the existing options
                {
                    symbols.Add(y);
                    xs.Add(y);
                }
            }

            var msg = string.Format("There are {0} incompatible declarations of the symbol {1}. They originate from {2}.",
                                    xs.Count, first.Name, symbols.Select(s => s.Origin).ToEnumerationString());
            return new KeyValuePair<string, Symbol>(first.Name,
                                                    Symbol.CreateMessage(Message.Create(MessageSeverity.Error, msg, first.Origin.Position,
                                                                                MessageClasses.SymbolConflict), xs[0].Symbol));
        }

        private static readonly ISymbolHandler<Message, bool> _containsMessage = new ContainsMessageHandler();

        /// <summary>
        /// Determines whether a symbol contains a message or not.
        /// </summary>
        private class ContainsMessageHandler : ISymbolHandler<Message, bool>
        {
            public bool HandleReference(ReferenceSymbol self, Message argument)
            {
                return false;
            }

            public bool HandleNil(NilSymbol self, Message argument)
            {
                return false;
            }

            public bool HandleExpand(ExpandSymbol self, Message argument)
            {
                return false;
            }

            public bool HandleNamespace(NamespaceSymbol self, Message argument)
            {
                return false;
            }

            public bool HandleMessage(MessageSymbol self, Message argument)
            {
                if (self.Message.Equals(argument))
                    return true;
                else
                    return self.InnerSymbol.HandleWith(this, argument);
            }

            public bool HandleDereference(DereferenceSymbol self, Message argument)
            {
                return self.InnerSymbol.HandleWith(this, argument);
            }
        }

        private static readonly ISymbolHandler<MergeContext, Symbol> _mergeHandler = new MergeHandler();

        private static Symbol _merge(SymbolInfo thisSymbol, SymbolInfo otherSymbol)
        {
            return new MergeContext {ThisInfo = thisSymbol, OtherInfo = otherSymbol}.Merge();
        }

        private sealed class MergeContext
        {
            public SymbolInfo ThisInfo { get; set; }
            public SymbolInfo OtherInfo { get; set; }

            public MergeContext Invert()
            {
                return new MergeContext{ThisInfo = OtherInfo, OtherInfo = ThisInfo};
            }

            public Symbol Merge()
            {
                return ThisInfo.Symbol.HandleWith(_mergeHandler, this);
            }
        }

        private sealed class MergeHandler : ISymbolHandler<MergeContext, Symbol>
        {
            public Symbol HandleReference(ReferenceSymbol thisSymbol, MergeContext mergeContext)
            {
                return _handleSymbol(thisSymbol, mergeContext);
            }

            public Symbol HandleNil(NilSymbol thisSymbol, MergeContext mergeContext)
            {
                return _handleSymbol(thisSymbol, mergeContext);
            }

            public Symbol HandleExpand(ExpandSymbol thisSymbol, MergeContext mergeContext)
            {
                return _handleSymbol(thisSymbol, mergeContext);
            }

            public Symbol HandleDereference(DereferenceSymbol thisSymbol, MergeContext mergeContext)
            {
                return _handleSymbol(thisSymbol, mergeContext);
            }

            private Symbol _handleSymbol(Symbol thisSymbol, MergeContext mergeContext)
            {
                // In general, non-message symbols must be equal modulo messages.
                var messageSymbol = mergeContext.OtherInfo.Symbol as MessageSymbol;
                if (messageSymbol != null)
                    return HandleMessage(messageSymbol, mergeContext.Invert());
                else
                    return thisSymbol.Equals(mergeContext.OtherInfo.Symbol) ? thisSymbol : null;
            }

            public Symbol HandleMessage(MessageSymbol thisSymbol, MergeContext mergeContext)
            {
                if (ReferenceEquals(thisSymbol, mergeContext.OtherInfo.Symbol))
                    return thisSymbol;

                // merge recursively
                var innerInfo = new SymbolInfo(thisSymbol.InnerSymbol, mergeContext.ThisInfo.Origin,
                    mergeContext.ThisInfo.Name);
                var innerUnionSymbol = thisSymbol.InnerSymbol.HandleWith(this,
                    new MergeContext
                    {
                        ThisInfo = innerInfo,
                        OtherInfo = mergeContext.OtherInfo
                    });
                if (innerUnionSymbol == null) // the underlying self is not the same
                    return null;

                if (innerUnionSymbol.HandleWith(_containsMessage, thisSymbol.Message))
                    return innerUnionSymbol;
                else
                    return Symbol.CreateMessage(thisSymbol.Message, innerUnionSymbol);
            }

            public Symbol HandleNamespace(NamespaceSymbol self, MergeContext mergeContext)
            {
                var otherSymbol = mergeContext.OtherInfo.Symbol;
                var messageSymbol = otherSymbol as MessageSymbol;
                if (messageSymbol != null)
                    return HandleMessage(messageSymbol, mergeContext.Invert());
                else if (self.Equals(otherSymbol))
                {
                    return self;
                }
                else
                {
                    var otherNamespaceSymbol = otherSymbol as NamespaceSymbol;
                    if (otherNamespaceSymbol == null)
                        return null;
                    else
                    {
                        // Two distinct namespaces collide
                        // Create a merged view of the namespace
                        var exportedFromThis = _exportedFrom(self, mergeContext.ThisInfo);
                        var exportedFromOther = _exportedFrom(otherNamespaceSymbol, mergeContext.OtherInfo);
                        var merged = new MergedNamespace(Create(conflictUnionSource:
                                        exportedFromThis.Append(exportedFromOther)));
                        return Symbol.CreateNamespace(merged, self.LogicalName ?? otherNamespaceSymbol.LogicalName,
                            mergeContext.ThisInfo.Origin.Position);
                    }
                }
            }

            private IEnumerable<SymbolInfo> _exportedFrom(NamespaceSymbol nsSymbol, SymbolInfo nsInfo)
            {
                return nsSymbol.Namespace.Select(entry => new SymbolInfo(entry.Value,nsInfo.Origin,entry.Key));
            }
        }

        private bool _notInUnion(KeyValuePair<string, Symbol> entry)
        {
            return !_union.ContainsKey(entry.Key);
        }

        private bool _notInLocal(KeyValuePair<string, Symbol> entry)
        {
            return !_local.ContainsKey(entry.Key);
        }

        private bool _notInLocalAndUnion(KeyValuePair<string, Symbol> entry)
        {
            var key = entry.Key;
            return !_union.ContainsKey(key) && !_local.ContainsKey(key);
        }

        public override IEnumerator<KeyValuePair<string, Symbol>> GetEnumerator()
        {
            var local = Volatile.Read(ref _local);
            if (local == null)
                if (_union == null)
                    if (_parent == null)
                        return Enumerable.Empty<KeyValuePair<string, Symbol>>().GetEnumerator();
                    else
                        return _parent.GetEnumerator();
                else if (_parent == null)
                    return _union.GetEnumerator();
                else
                    return _union.Append(_parent.Where(_notInUnion)).GetEnumerator();
            else
            {
                return _readLockEnumerable(_assembleEnumerator(local));
            }
        }

        private IEnumerator<KeyValuePair<string, Symbol>> _readLockEnumerable(IEnumerable<KeyValuePair<string, Symbol>> sequence)
        {
            _lock.EnterReadLock();
            try
            {
                foreach (var item in sequence)
                    yield return item;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        private IEnumerable<KeyValuePair<string, Symbol>> _assembleEnumerator(IEnumerable<KeyValuePair<string, Symbol>> local)
        {
            if (_union == null)
                if (_parent == null)
                    return local;
                else
                    return local.Append(_parent.Where(_notInLocal));
            else if (_parent == null)
                return local.Append(_union.Where(_notInLocal));
            else
                return
                    local.Append(_union.Where(_notInLocal)).Append(_parent.Where(_notInLocalAndUnion));
        }

        public override bool TryGet(string id, out Symbol value)
        {
            var local = Volatile.Read(ref _local);
            if (local != null)
            {
                _lock.EnterReadLock();
                try
                {
                    if (local.TryGetValue(id, out value))
                        return true;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
            
            if (_union != null && _union.TryGetValue(id, out value))
            {
                return true;
            }

            if (_parent != null && _parent.TryGet(id, out value))
            {
                return true;
            }
            
            value = null;
            return false;
        }

        public override bool IsEmpty
        {
            get
            {
                var local = Volatile.Read(ref _local);
                bool localIsEmpty;
                if (local != null)
                {
                    _lock.EnterReadLock();
                    try
                    {
                        localIsEmpty = _local.Count > 0;
                    }
                    finally
                    {
                        _lock.ExitReadLock();
                    }
                }
                else
                {
                    localIsEmpty = true;
                }
                return
                    !((localIsEmpty)
                        || (_union != null && _union.Count > 0)
                        || (_parent != null && !_parent.IsEmpty));
            }
        }

        public override void Declare(string id, Symbol symbol)
        {
            _lock.EnterWriteLock();
            try
            {
                if (_local == null)
                    _local = new SymbolTable<Symbol>();
                _local[id] = symbol;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public override bool IsDeclaredLocally(string id)
        {
            var local = Volatile.Read(ref _local);
            if (local == null)
                return false;
            else
            {
                _lock.EnterReadLock();
                try
                {
                    return _local != null && _local.ContainsKey(id);
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        public override void ClearLocalDeclarations()
        {
            _lock.EnterWriteLock();
            try
            {
                _local = null;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public override IEnumerable<KeyValuePair<string, Symbol>> LocalDeclarations
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    if (_local != null)
                        foreach (var symbol in _local)
                            yield return symbol;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }
    }
}