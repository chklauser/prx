// Prexonite
// 
// Copyright (c) 2011, Christian Klauser
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
using Prexonite.Modular;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

namespace Prexonite.Compiler
{
    public class SymbolEntry : IEquatable<SymbolEntry>
    {
        #region Representation

        private readonly SymbolInterpretations _interpretation;
        private readonly string _id;
        private readonly ModuleName _module;

        /// <summary>
        ///     Optional integer parameter for this symbol.
        /// 
        ///     Label - Address of jump target (if known)
        /// </summary>
        private int? _argument;

        #endregion

        #region Constructor

        public SymbolEntry(SymbolInterpretations interpretation, ModuleName module)
        {
            _interpretation = interpretation;
            _module = module;
            _id = null;
        }

        public SymbolEntry(SymbolInterpretations interpretation, string id, ModuleName module)
            : this(interpretation, module)
        {
            if (id != null && id.Length <= 0)
                id = null;
            _id = id;
        }

        public SymbolEntry(SymbolInterpretations interpretation, int? argument, ModuleName module)
            : this(interpretation, module)
        {
            _argument = argument;
        }

        public SymbolEntry(SymbolInterpretations interpretation, string id, int? argument, ModuleName module)
            : this(interpretation, id, module)
        {
            _argument = argument;
        }

        #endregion

        #region Public accessors

        public SymbolInterpretations Interpretation
        {
            get { return _interpretation; }
        }

        public string InternalId
        {
            get { return _id; }
        }

        public ModuleName Module
        {
            get { return _module; }
        }

        /// <summary>
        ///     An optional intger parameter for this symbol. Use determined by symbol interpretation.
        /// </summary>
        public int? Argument
        {
            get { return _argument; }
        }

        #endregion

        public SymbolEntry WithModule(ModuleName module, SymbolInterpretations? interpretation = null,
            string translatedId = null, int? argument = null)
        {
            return new SymbolEntry(interpretation ?? Interpretation, translatedId ?? InternalId,
                argument ?? Argument, module);
        }

        public SymbolEntry With(SymbolInterpretations? interpretation = null, 
            string translatedId = null, int? argument = null)
        {
            return new SymbolEntry(interpretation ?? Interpretation, translatedId ?? InternalId,
                argument ?? Argument, Module);
        }

        public SymbolEntry With(SymbolInterpretations interpretation, string translatedId)
        {
            return new SymbolEntry(interpretation, translatedId, Argument, Module);
        }

        public SymbolEntry With(SymbolInterpretations interpretation)
        {
            return new SymbolEntry(interpretation, InternalId, Argument, Module);
        }

        public override string ToString()
        {
            return Enum.GetName(
                typeof (SymbolInterpretations), Interpretation) +
                    (InternalId == null ? "" : ":" + InternalId) + 
                    (_argument.HasValue ? "#" + _argument.Value : "") +
                    (Module != null ? ("," + Module) : "");
        }

        #region Equality

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(SymbolEntry other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._interpretation, _interpretation) && Equals(other._id, _id) && Equals(other._module, _module) && other._argument.Equals(_argument);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (SymbolEntry)) return false;
            return Equals((SymbolEntry) obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = _interpretation.GetHashCode();
                result = (result*397) ^ (_id != null ? _id.GetHashCode() : 0);
                result = (result*397) ^ (_module != null ? _module.GetHashCode() : 0);
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

        #endregion

#region Factory methods

        public static SymbolEntry LocalObjectVariable(string id)
        {
            return new SymbolEntry(SymbolInterpretations.LocalObjectVariable, id,null);
        }

        public static SymbolEntry LocalReferenceVariable(string id)
        {
            return new SymbolEntry(SymbolInterpretations.LocalReferenceVariable, id, null);
        }

        public static SymbolEntry Command(string id)
        {
            return new SymbolEntry(SymbolInterpretations.Command,id,null);
        }

        public static SymbolEntry MacroCommand(string id)
        {
            return new SymbolEntry(SymbolInterpretations.MacroCommand, id, null);
        }

        public static SymbolEntry JumpLabel(int address)
        {
            return new SymbolEntry(SymbolInterpretations.JumpLabel, address, null);
        }

#endregion

    }

    /// <summary>
    ///     Determines the meaning of a symbol.
    /// </summary>
    public enum SymbolInterpretations
    {
        /// <summary>
        ///     Due to an error, the interpretation of this symbol could not be determined.
        /// </summary>
        Undefined = -1,

        /// <summary>
        ///     Symbol has no interpretation/meaning.
        /// </summary>
        None = 0,

        /// <summary>
        ///     Symbol is a function, to be looked up in <see cref = "Application.Functions" />.
        /// </summary>
        Function,

        /// <summary>
        ///     Symbol is a command, to be looked up in <see cref = "Engine.Commands" />.
        /// </summary>
        Command,

        /// <summary>
        ///     Symbol is a known type, to be looked up in <see cref = "Engine.PTypeRegistry" />.
        /// </summary>
        KnownType,

        /// <summary>
        ///     Symbol is a jump label, resolved during code emission.
        /// </summary>
        JumpLabel,

        /// <summary>
        ///     Symbol is a local variable.
        /// </summary>
        LocalObjectVariable,

        /// <summary>
        ///     Symbol is a local variable, will be dereferenced when mentioned. See <see cref = "IIndirectCall" />.
        /// </summary>
        LocalReferenceVariable,

        /// <summary>
        ///     Symbol is a global variable, to be looked up in <see cref = "Application.Variables" />.
        /// </summary>
        GlobalObjectVariable,

        /// <summary>
        ///     Symbol is a global variable, to be looked up in <see cref = "Application.Variables" />, 
        ///     will be dereferenced when mentioned. See <see cref = "IIndirectCall" />.
        /// </summary>
        GlobalReferenceVariable,

        /// <summary>
        ///     Symbol is a macro command, to be looked up in <see cref = "Loader.MacroCommands" />. 
        ///     Macro commands need to be applied at compile-time.
        /// </summary>
        MacroCommand
    }
}