/*
 * Prexonite, a scripting engine (Scripting Language -> Bytecode -> Virtual Machine)
 *  Copyright (C) 2007  Christian "SealedSun" Klauser
 *  E-mail  sealedsun a.t gmail d.ot com
 *  Web     http://www.sealedsun.ch/
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  Please contact me (sealedsun a.t gmail do.t com) if you need a different license.
 * 
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

using System;
using System.Collections.Generic;
using Prexonite.Types;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

namespace Prexonite
{
    /// <summary>
    /// Represents an "address" in the Prexonite VM.
    /// </summary>
    /// <remarks>
    /// <para>
    ///     PVariables are used for both local and global variables with the difference that 
    ///     local variables don't usually have a <see cref="Meta">MetaTable</see>.
    /// </para>
    /// <para>
    ///     Also, variable references are just references to the PVariable objects applications or functions.
    /// </para>
    /// </remarks>
    [NoDebug()]
    public sealed class PVariable : IMetaFilter,
                                    IHasMetaTable,
                                    IIndirectCall
    {
        private PValue _value;
        //Variable metatables are only created when requested.
        private MetaTable _meta;

        /// <summary>
        /// Provides readonly access to the variable's <see cref="MetaTable"/>.
        /// </summary>
        /// <remarks>The default constructor does not create a <see cref="MetaTable"/> but as soon as this property is accessed for the first time, one will be instantiated.</remarks>
        public MetaTable Meta
        {
            get
            {
                if (_meta == null)
                {
                    _meta = new MetaTable(this);
                    _meta[Application.NameKey] = Engine.GenerateName();
                }
                return _meta;
            }
        }

        /// <summary>
        /// Provides access to the value stored in the variable.
        /// </summary>
        /// <value>The PValue object stored in this variables or a PValue(null) object if the reference is null.</value>
        public PValue Value
        {
            get { return _value ?? PType.Null.CreatePValue(); }
            set { this._value = value; }
        }

        #region IMetaFilter Members

        string IMetaFilter.GetTransform(string key)
        {
            return key;
        }

        KeyValuePair<string, MetaEntry>? IMetaFilter.SetTransform(
            KeyValuePair<string, MetaEntry> item)
        {
            //The name property may not be reset
            if (Engine.StringsAreEqual(item.Key, Application.NameKey))
                return null;

            return item;
        }

        #endregion

        /// <summary>
        /// Creates a new (local) variable.
        /// </summary>
        /// <remarks>
        /// This constructor does not create a <see cref="MetaTable"/>.
        /// </remarks>
        public PVariable()
        {
        }

        /// <summary>
        /// Creates a new (global) variable.
        /// </summary>
        /// <param name="name">An identifier to be stored in the variable's <see cref="Meta">MetaTable</see>.</param>
        /// <remarks>This constructor creates a <see cref="MetaTable"/>.</remarks>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="name"/> is empty.</exception>
        public PVariable(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (name.Length == 0)
                throw new ArgumentException("name is expected to contain at least one character.");
            Meta[Application.NameKey] = name;
        }

        #region IIndirectCall Members

        /// <summary>
        /// "Calling" a variable means "getting" or "setting" the variable's value.
        /// </summary>
        /// <param name="sctx">The stack context in which to perform the indirect call.</param>
        /// <param name="args">The list of arguments to be passed to this indirect call. <br />
        /// Only the first element of the array is ever used in by this special implementation of <see cref="IIndirectCall.IndirectCall"/>.</param>
        /// <remarks>If <paramref name="args"/> is empty, just the variable's current value is returned.<br />
        /// Otherwise, the first element of <paramref name="args"/> is assigned to the variable.</remarks>
        /// <returns>Always the variable's (new) value.</returns>
        public PValue IndirectCall(StackContext sctx, PValue[] args)
        {
            if (args.Length != 0)
                _value = args[args.Length - 1];
            return _value;
        }

        #endregion

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj);
        }

        public override int GetHashCode()
        {
            return _value == null ? -12 : _value.GetHashCode() ^ 6537;
        }
    }
}