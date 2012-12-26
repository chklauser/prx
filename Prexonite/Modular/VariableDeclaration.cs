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
