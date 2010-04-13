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
    public class SymbolEntry
    {
        public SymbolInterpretations Interpretation;
        public string Id;
        public int? Argument;

        public SymbolEntry(SymbolInterpretations interpretation)
        {
            Interpretation = interpretation;
            Id = null;
        }

        public SymbolEntry(SymbolInterpretations interpretation, string id)
            : this(interpretation)
        {
            if (id != null && id.Length <= 0)
                id = null;
            Id = id;
        }

        public SymbolEntry(SymbolInterpretations interpretation, int? argument)
            : this(interpretation)
        {
            if (argument.HasValue)
                Argument = argument.Value;
            else
                Argument = null;
        }

        public static SymbolEntry CreateHiddenSymbol(SymbolInterpretations interpretation)
        {
            return new SymbolEntry(interpretation, "H\\" + Guid.NewGuid().ToString("N"));
        }

        public static bool operator ==(SymbolEntry entry1, SymbolEntry entry2)
        {
            return ReferenceEquals(entry1, entry2);
        }

        public static bool operator !=(SymbolEntry entry1, SymbolEntry entry2)
        {
            return !(entry1 == entry2);
        }

        public override int GetHashCode()
        {
            return (
                       Enum.GetName(typeof (SymbolInterpretations), Interpretation) +
                       (Id ?? "-\\/_\\/-") +
                       (!Argument.HasValue
                            ?
                                "\\-//\\-//\\"
                            :
                                Argument.ToString()
                       )).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            else if (obj is SymbolEntry)
            {
                var entry = (SymbolEntry) obj;
                return
                    Id == entry.Id &&
                    Argument == entry.Argument &&
                    Interpretation == entry.Interpretation;
            }
            else
                return base.Equals(obj);
        }

        public override string ToString()
        {
            return Enum.GetName(
                       typeof (SymbolInterpretations), Interpretation) +
                   (Id == null ? "" : "->" + Id) +
                   (Argument.HasValue ? "#" + Argument.Value : ""
                   );
        }
    }

    public enum SymbolInterpretations
    {
        Undefined = -1,
        None = 0,
        Function,
        Command,
        KnownType,
        JumpLabel,
        LocalObjectVariable,
        LocalReferenceVariable,
        GlobalObjectVariable,
        GlobalReferenceVariable
    }
}