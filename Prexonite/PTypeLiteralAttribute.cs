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
using System.Diagnostics;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

namespace Prexonite.Types
{
    /// <summary>
    /// Associates a literal with a class. Only interpreted on classes inheriting from <see cref="Prexonite.Types.PType"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    [DebuggerStepThrough]
    public class PTypeLiteralAttribute : Attribute
    {
        private readonly string _literal;

        /// <summary>
        /// The literal this attribute represents.
        /// </summary>
        public string Literal
        {
            get { return _literal; }
        }

        /// <summary>
        /// Creates a new instance of the PTypeLiteral attribute.
        /// </summary>
        /// <param name="literal">The literal to associate with this type.</param>
        public PTypeLiteralAttribute(string literal)
        {
            _literal = literal;
        }
    }
}