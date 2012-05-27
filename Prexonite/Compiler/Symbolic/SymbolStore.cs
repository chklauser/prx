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

using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Prexonite.Compiler.Symbolic.Internal;

namespace Prexonite.Compiler.Symbolic
{
    /// <summary>
    /// Represents a symbol declaration and lookup scope. A SymbolStore could stand alone or
    /// it could forward queries to one or more "surrounding"/"parent" scopes. It could also
    /// present the intersection or union of multiple scopes.
    /// </summary>
    public abstract class SymbolStore : ISymbolView<Symbol>
    {
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Lazily enumerates all symbols visible from this scope.
        /// </summary>
        /// <returns>An enumeration that lists all symbols visible from this scope.</returns>
        [PublicAPI]
        public abstract IEnumerator<KeyValuePair<string, Symbol>> GetEnumerator();

        /// <summary>
        /// Looks up a symbol entry in this scope. On success, it stores the result in <paramref name="value"/>.
        /// Does not generate error symbols for non-existant symbols. It might, however, return error symbols
        /// for other error conditions (such as symbol name conflicts).
        /// </summary>
        /// <param name="id">The symbolic ID to look up.</param>
        /// <param name="value">On success, contains the symbol. Otherwise undefined.</param>
        /// <returns>True on success; false otherwise.</returns>
        [PublicAPI]
        public abstract bool TryGet(string id, out Symbol value);

        /// <summary>
        /// Returns the total number of symbols visible from this scope.
        /// </summary>
        [PublicAPI]
        public abstract int Count { get; }

        /// <summary>
        /// Creates a new <see cref="SymbolStore"/> using a default implementation.
        /// </summary>
        /// <param name="parent">The symbol store where failed queries are forwarded to. Can be null.</param>
        /// <param name="conflictUnionSource">A sequence of stores that the newly created store should provide a unified view of.</param>
        /// <returns>A new symbol store.</returns>
        [PublicAPI]
        public static SymbolStore Create([CanBeNull] SymbolStore parent = null, [CanBeNull] IEnumerable<SymbolInfo> conflictUnionSource = null)
        {
            return new ConflictUnionFallbackStore(parent,conflictUnionSource);
        }

        /// <summary>
        /// Add a symbol entry that shadows all existing declarations with the same symbolic id.
        /// </summary>
        /// <param name="id">The symbolic id for this entry.</param>
        /// <param name="symbol">The actual symbol entry to be added to the store.</param>
        [PublicAPI]
        public abstract void Declare(string id, Symbol symbol);

        /// <summary>
        /// Indicates whether a declaration for the supplied symbolic id exists locally (was declared via <see cref="Declare"/>.
        /// </summary>
        /// <param name="id">The symbolic id to look up.</param>
        /// <returns>True if there is a local definition for <paramref name="id"/>; false if there is no definition for <paramref name="id"/> or if the only definition(s) for <paramref name="id"/> are not local.</returns>
        [PublicAPI]
        public abstract bool IsDeclaredLocally(string id);

        //TODO Find a good place for the method CreateSymbolNotFoundError
        /// <summary>
        /// Creates a new error symbol indicating that the supplied symbolic id could not be resolved.
        /// </summary>
        /// <param name="id">The symbolic id that could not be resolved.</param>
        /// <param name="position">The source position where to report the error.</param>
        /// <returns>An error symbol with a message indicating that the supplied id could not be resolved.</returns>
        internal static Symbol _CreateSymbolNotFoundError(string id, ISourcePosition position)
        {
            var msg = Message.Error(string.Format("Cannot resolve symbol {0}.", id), position,
                                    MessageClasses.SymbolNotResolved);
            return new MessageSymbol(msg, null);
        }
    }
}
