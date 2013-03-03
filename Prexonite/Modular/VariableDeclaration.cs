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
using Prexonite.Properties;

namespace Prexonite.Modular
{
    public abstract class VariableDeclaration : IHasMetaTable, IMetaFilter
    {
        /// <summary>
        /// The id of the global variable. Not null and not empty.
        /// </summary>
        public abstract string Id { get; }

        /// <summary>
        /// The meta table for this global variable.
        /// </summary>
        public abstract MetaTable Meta { get; }

        string IMetaFilter.GetTransform(string key)
        {
            if (Engine.StringsAreEqual(key, Application.NameKey))
                return Application.IdKey;
            else
                return key;
        }

        KeyValuePair<string, MetaEntry>? IMetaFilter.SetTransform(KeyValuePair<string, MetaEntry> item)
        {
            if (Engine.StringsAreEqual(item.Key, Application.NameKey))
                return new KeyValuePair<string, MetaEntry>(Application.IdKey, item.Value);
            else
                return item;
        }

        /// <summary>
        /// Creates a new instance of <see cref="VariableDeclaration"/> using a default implementation.
        /// </summary>
        /// <param name="id">The id of the variable to declare.</param>
        /// <returns>A variable declaration with the specified id and an empty meta table (except for the Id entry).</returns>
        public static VariableDeclaration Create(string id)
        {
            return new Impl(id);
        }

        /// <summary>
        /// Declaration of a Prexonite (global) variable.
        /// </summary>
        [System.Diagnostics.DebuggerDisplay("var {Id}")]
        private sealed class Impl : VariableDeclaration
        {
            /// <summary>
            /// Creates a new global variable declaration.
            /// </summary>
            /// <param name="id">The id of the global variable.</param>
            /// <exception cref="ArgumentNullException">id is null</exception>
            /// <exception cref="ArgumentException">id is empty</exception>
            public Impl(string id)
            {
                if (id == null)
                    throw new ArgumentNullException("id");
                if (id.Length == 0)
                    throw new ArgumentException(Resources.VariableDeclaration_Variable_id_must_not_be_empty, "id");

                _metaTable[Application.IdKey] = id;
            }

            private readonly MetaTable _metaTable = MetaTable.Create();

            /// <summary>
            /// The id of the global variable. Not null and not empty.
            /// </summary>
            public override string Id
            {
                get { return Meta[Application.IdKey].Text; }
            }

            /// <summary>
            /// The meta table for this global variable.
            /// </summary>
            public override MetaTable Meta
            {
                get { return _metaTable; }
            }
        }
    }

    
}
