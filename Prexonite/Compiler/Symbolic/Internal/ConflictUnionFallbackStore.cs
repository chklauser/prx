// Prexonite
// 
// Copyright (c) 2012, Christian Klauser
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
using System.Linq;
using System.Threading;

namespace Prexonite.Compiler.Symbolic.Internal
{
    internal class ConflictUnionFallbackStore : SymbolStore
    {
        internal ConflictUnionFallbackStore(SymbolStore parent = null, IEnumerable<SymbolInfo> conflictUnionSource = null)
        {
            _parent = parent;

            if(conflictUnionSource != null)
            {
                var u = new SymbolTable<Symbol>();
                u.AddRange(conflictUnionSource.GroupBy(i => i.Name,Engine.DefaultStringComparer).Select(_unifySymbols));
                if (u.Count > 0)
                    _union = u;
            }

            if (_union != null)
                if (_parent != null)
                    _externCount = _union.Count + _parent.Where(_notInUnion).Count();
                else
                    _externCount = _union.Count;
            else if (_parent != null)
                _externCount = _parent.Count;
            else
                _externCount = 0;
        }

        private static KeyValuePair<string,Symbol> _unifySymbols(IGrouping<string,SymbolInfo> source)
        {
            using (var e = source.GetEnumerator())
            {
                if(!e.MoveNext())
// ReSharper disable NotResolvedInText
                    throw new ArgumentOutOfRangeException("conflictUnionSource",source.Key,"Invalid key in source for symbol store.");
// ReSharper restore NotResolvedInText

                var unionInfo = e.Current;
                var x = unionInfo.Symbol;

                while (e.MoveNext())
                {
                    var y = e.Current;
                    var merged = x.HandleWith(_merge, y.Symbol);
                    if (merged == null)
                        return _unifySymbolsDualMode(new SymbolInfo(x, unionInfo.Origin, unionInfo.Name), y, e);
                    else
                        x = merged;
                }

                return new KeyValuePair<string, Symbol>(unionInfo.Name,x);
            }
        }

        private static KeyValuePair<string,Symbol> _unifySymbolsDualMode(SymbolInfo first, SymbolInfo second, IEnumerator<SymbolInfo> e)
        {
            var x1 = first.Symbol;
            var x2 = second.Symbol;

            while (e.MoveNext())
            {
                var y = e.Current;

                var merged = x1.HandleWith(_merge, y.Symbol);
                if (merged != null)
                {
                    x1 = merged;
                }
                else
                {
                    merged = x2.HandleWith(_merge, y.Symbol);
                    if (merged != null)
                    {
                        x2 = merged;
                    }
                    else
                    {
                        return _unifySymbolsMultiMode(new SymbolInfo(x1, first.Origin, first.Name),
                                                      new SymbolInfo(x2, second.Origin, second.Name), y, e);
                    }
                }
            }

            var msg = string.Format("There are two incompatible declarations of the symbol {0} in this scope. " +
                                    "One comes from {1}, the other one from {2}.", first.Name, first.Origin,
                                    second.Origin);

            return new KeyValuePair<string, Symbol>(first.Name,
                                                    new MessageSymbol(
                                                        new Message(MessageSeverity.Error,
                                                                    msg,
                                                                    first.Origin,
                                                                    MessageClasses.SymbolConflict), x1));
        }

        private static KeyValuePair<string, Symbol> _unifySymbolsMultiMode(SymbolInfo first, SymbolInfo second, SymbolInfo third, IEnumerator<SymbolInfo> e)
        {
            var symbols = new List<SymbolInfo> {first, second, third};
            var xs = new List<Symbol> {first.Symbol, second.Symbol, third.Symbol};

            while (e.MoveNext())
            {
                var y = e.Current;
                int i;
                for (i = 0; i < xs.Count; i++)
                {
                    var merged = xs[i].HandleWith(_merge, y.Symbol);
                    if (merged != null)
                    {
                        xs[i] = merged;
                        break;
                    }
                }

                if(i == xs.Count) // did not unify with any of the existing options
                {
                    symbols.Add(y);
                    xs.Add(y.Symbol);
                }
            }

            var msg = string.Format("There are {0} incompatible declarations of the symbol {1}. They come from {2}.",
                                    xs.Count, first.Name, symbols.Select(s => s.Origin).ToEnumerationString());
            return new KeyValuePair<string, Symbol>(first.Name,
                                                    new MessageSymbol(
                                                        new Message(MessageSeverity.Error, msg, first.Origin,
                                                                    MessageClasses.SymbolConflict), xs[0]));
        }

        private static readonly ISymbolHandler<Object, bool> _containsMessage = new ContainsMessageHandler();
        private class ContainsMessageHandler : ISymbolHandler<Object,bool>
        {
            public bool HandleEntity(EntitySymbol symbol, object argument)
            {
                return false;
            }

            public bool HandleMessage(MessageSymbol symbol, object argument)
            {
                if (symbol.Message == argument)
                    return true;
                else
                    return symbol.Symbol.HandleWith(this,argument);
            }

            public bool HandleMacroInstance(MacroInstanceSymbol symbol, object argument)
            {
                return false;
            }
        }

        private static readonly ISymbolHandler<Symbol,Symbol> _merge = new MergeHandler(); 
        private class MergeHandler : ISymbolHandler<Symbol, Symbol>
        {
            public Symbol HandleEntity(EntitySymbol thisSymbol, Symbol otherSymbol)
            {
                var messageSymbol = otherSymbol as MessageSymbol;
                if (messageSymbol != null)
                    return HandleMessage(messageSymbol, thisSymbol);
                else
                    return thisSymbol.Equals(otherSymbol) ? thisSymbol : null;
            }

            public Symbol HandleMessage(MessageSymbol thisSymbol, Symbol otherSymbol)
            {
                if (ReferenceEquals(thisSymbol, otherSymbol))
                    return thisSymbol;

                var innerUnionSymbol = thisSymbol.Symbol.HandleWith(this, otherSymbol);
                if (innerUnionSymbol == null) // the underlying symbol is not the same
                    return null;

                if (innerUnionSymbol.HandleWith(_containsMessage, thisSymbol.Message))
                    return innerUnionSymbol;
                else
                    return new MessageSymbol(thisSymbol.Message, innerUnionSymbol);
            }

            public Symbol HandleMacroInstance(MacroInstanceSymbol thisSymbol, Symbol otherSymbol)
            {
                //Macro instances cannot be merged, they must be identical (except for messages)
                if (ReferenceEquals(thisSymbol, otherSymbol))
                    return thisSymbol;
                
                var messageSymbol = otherSymbol as MessageSymbol;
                if (messageSymbol != null)
                    return HandleMessage(messageSymbol, thisSymbol);
                else 
                    return null;
            }
        }

        private readonly SymbolStore _parent;
        private readonly SymbolTable<Symbol> _union;
        private SymbolTable<Symbol> _local;
        private readonly int _externCount;

        private bool _notInUnion(KeyValuePair<string,Symbol> entry)
        {
            return !_union.ContainsKey(entry.Key);
        }

        private bool _notInLocal(KeyValuePair<string,Symbol> entry )
        {
            return !_local.ContainsKey(entry.Key);
        }

        private bool _notInLocalAndUnion(KeyValuePair<string,Symbol>  entry)
        {
            var key = entry.Key;
            return !_union.ContainsKey(key) && !_local.ContainsKey(key);
        }

        public override IEnumerator<KeyValuePair<string, Symbol>> GetEnumerator()
        {
            if (_local == null)
                if (_union == null)
                    if (_parent == null)
                        return Enumerable.Empty<KeyValuePair<string, Symbol>>().GetEnumerator();
                    else
                        return _parent.GetEnumerator();
                else if (_parent == null)
                    return _union.GetEnumerator();
                else
                    return _union.Append(_parent.Where(_notInUnion)).GetEnumerator();
            else if (_union == null)
                if (_parent == null)
                    return _local.GetEnumerator();
                else
                    return _local.Append(_parent.Where(_notInLocal)).GetEnumerator();
            else if (_parent == null)
                return _local.Append(_union.Where(_notInLocal)).GetEnumerator();
            else
                return
                    _local.Append(_union.Where(_notInLocal)).Append(_parent.Where(_notInLocalAndUnion)).
                        GetEnumerator();
        }

        public override bool TryGet(string id, out Symbol value)
        {
            // This could certainly be written more tersely, but
            //  after the JIT compiler, both forms look the same anyway
            //  so we can as well leave it in a more read-able form.
            if(_local != null && _local.TryGetValue(id, out value))
            {
                return true;
            }
            else if(_union != null && _union.TryGetValue(id,out value))
            {
                return true;
            }
            else if(_parent != null && _parent.TryGet(id, out value))
            {
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }

        public override int Count
        {
            get { return _local == null ? _externCount : _externCount + _local.Count; }
        }

        public override void Declare(string id, Symbol symbol)
        {
            if (_local == null)
                Interlocked.CompareExchange(ref _local, new SymbolTable<Symbol>(), null);
            _local[id] = symbol;
        }

        public override bool IsDeclaredLocally(string id)
        {
            return _local == null || _local.ContainsKey(id);
        }

        public override void ClearLocalDeclarations()
        {
            _local = null;
        }

        public override IEnumerable<KeyValuePair<string, Symbol>> LocalDeclarations
        {
            get { return _local ?? Enumerable.Empty<KeyValuePair<string,Symbol>>(); }
        }
    }
}