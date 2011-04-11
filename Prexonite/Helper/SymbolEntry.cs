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
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

namespace Prexonite.Compiler
{
    public class SymbolEntry : IEquatable<SymbolEntry>
    {
        private SymbolInterpretations _interpretation;
        private string _id;
        /// <summary>
        /// Optional integer parameter for this symbol.
        /// 
        /// Label - Address of jump target (if known)
        /// </summary>
        private int? _argument;

        public SymbolEntry(SymbolInterpretations interpretation)
        {
            _interpretation = interpretation;
            _id = null;
        }

        public SymbolEntry(SymbolInterpretations interpretation, string id)
            : this(interpretation)
        {
            if (id != null && id.Length <= 0)
                id = null;
            _id = id;
        }

        public SymbolEntry(SymbolInterpretations interpretation, int? argument)
            : this(interpretation)
        {
            _argument = argument;
        }

        public SymbolEntry(SymbolInterpretations interpretation, string id, int? argument)
            : this(interpretation, id)
        {
            _argument = argument;
        }

        public SymbolInterpretations Interpretation
        {
            get { return _interpretation; }
        }

        public string Id
        {
            get { return _id; }
        }

        /// <summary>
        /// An optional intger parameter for this symbol. Use determined by symbol interpretation.
        /// </summary>
        public int? Argument
        {
            get { return _argument; }
        }

        public SymbolEntry With(SymbolInterpretations interpretation, string translatedId)
        {
            return new SymbolEntry(interpretation, translatedId, Argument);
        }

        public SymbolEntry With(SymbolInterpretations interpretation)
        {
            return new SymbolEntry(interpretation, Id, Argument);
        }

        public override string ToString()
        {
            return Enum.GetName(
                       typeof (SymbolInterpretations), Interpretation) +
                   (Id == null ? "" : "->" + Id) +
                   (_argument.HasValue ? "#" + _argument.Value : ""
                   );
        }

        public bool Equals(SymbolEntry other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._interpretation, _interpretation) && Equals(other._id, _id) && other._argument.Equals(_argument);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (SymbolEntry)) return false;
            return Equals((SymbolEntry) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = _interpretation.GetHashCode();
                result = (result*397) ^ (_id != null ? _id.GetHashCode() : 0);
                result = (result*397) ^ (_argument.HasValue ? _argument.Value : 0);
                return result;
            }
        }

        public static bool operator ==(SymbolEntry left, SymbolEntry right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SymbolEntry left, SymbolEntry right)
        {
            return !Equals(left, right);
        }
    }

    /// <summary>
    /// Determines the meaning of a symbol.
    /// </summary>
    public enum SymbolInterpretations
    {
        /// <summary>
        /// Due to an error, the interpretation of this symbol could not be determined.
        /// </summary>
        Undefined = -1,
        /// <summary>
        /// Symbol has no interpretation/meaning.
        /// </summary>
        None = 0,
        /// <summary>
        /// Symbol is a function, to be looked up in <see cref="Application.Functions"/>.
        /// </summary>
        Function,
        /// <summary>
        /// Symbol is a command, to be looked up in <see cref="Engine.Commands"/>.
        /// </summary>
        Command,
        /// <summary>
        /// Symbol is a known type, to be looked up in <see cref="Engine.PTypeRegistry"/>.
        /// </summary>
        KnownType,
        /// <summary>
        /// Symbol is a jump label, resolved during code emission.
        /// </summary>
        JumpLabel,
        /// <summary>
        /// Symbol is a local variable.
        /// </summary>
        LocalObjectVariable,
        /// <summary>
        /// Symbol is a local variable, will be dereferenced when mentioned. See <see cref="IIndirectCall"/>.
        /// </summary>
        LocalReferenceVariable,
        /// <summary>
        /// Symbol is a global variable, to be looked up in <see cref="Application.Variables"/>.
        /// </summary>
        GlobalObjectVariable,
        /// <summary>
        /// Symbol is a global variable, to be looked up in <see cref="Application.Variables"/>, 
        /// will be dereferenced when mentioned. See <see cref="IIndirectCall"/>.
        /// </summary>
        GlobalReferenceVariable,
        /// <summary>
        /// Symbol is a macro command, to be looked up in <see cref="Loader.MacroCommands"/>. 
        /// Macro commands need to be applied at compile-time.
        /// </summary>
        MacroCommand
    }
}