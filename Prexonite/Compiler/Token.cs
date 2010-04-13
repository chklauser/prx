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

namespace Prexonite.Compiler
{
    internal class Token
    {
        internal int kind;
        internal string val;
        internal int pos;
        internal int line;
        internal int col;
        internal Token next;

        public Token()
        {
        }

        public Token(Token next)
        {
            this.next = next;
        }

        public override string ToString()
        {
            return ToString(true);
        }

        public string ToString(bool includePosition)
        {
            return
                String.Format(
                    "({0})~{1}" + (includePosition ? "/line:{2}/col:{3}" : ""),
                    val,
                    Enum.GetName(typeof (Parser.Terminals), (Parser.Terminals) kind),
                    line,
                    col);
        }
    }
}