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
using System.Reflection.Emit;
using cop = System.Reflection.Emit.OpCodes;

namespace Prexonite.Compiler.Cil
{
    public class Symbol
    {
        private SymbolKind _kind;

        private LocalBuilder _local;

        [DebuggerStepThrough]
        public Symbol(SymbolKind kind)
        {
            _kind = kind;
        }

        public SymbolKind Kind
        {
            [DebuggerStepThrough]
            get
            {
                return _kind;
            }
            [DebuggerStepThrough]
            set
            {
                _kind = value;
            }
        }

        public LocalBuilder Local
        {
            [DebuggerStepThrough]
            get
            {
                return _local;
            }
            [DebuggerStepThrough]
            set
            {
                _local = value;
            }
        }

        public void EmitLoad(CompilerState state)
        {
            switch(Kind)
            {
                case SymbolKind.Local:
                    state.EmitLoadLocal(Local.LocalIndex);
                    break;
                case SymbolKind.LocalRef:
                    state.EmitLoadLocal(Local.LocalIndex);
                    state.Il.EmitCall(cop.Call, Compiler.GetValueMethod, null);
                    break;
            }
        }
    }

    public enum SymbolKind
    {
        Local,
        LocalRef,
        LocalEnum
    }
}