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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Prexonite.Compiler.Cil
{
    public class ForeachHint
    {
        private readonly int castAddress;
        private readonly int disposeAddress;
        private readonly string enumVar;
        private readonly int getCurrentAddress;
        private readonly int moveNextAddress;

        public ForeachHint(
            string enumVar, int castAddress, int getCurrentAddress, int moveNextAddress, int disposeAddress)
        {
            this.enumVar = enumVar;
            this.disposeAddress = disposeAddress;
            this.moveNextAddress = moveNextAddress;
            this.getCurrentAddress = getCurrentAddress;
            this.castAddress = castAddress;
        }

        #region Meta format

        public const int CastAddressIndex = 2;
        public const int DisposeAddressIndex = 5;
        public const int EntryLength = 6;
        public const int EnumVarIndex = 1;
        public const int GetCurrentAddressIndex = 3;
        public const string Key = "foreach";
        public const int MoveNextAddressIndex = 4;

        public ForeachHint(MetaEntry[] hint)
        {
            if(hint == null)
                throw new ArgumentNullException("hint");
            if(hint.Length < EntryLength)
                throw new ArgumentException(string.Format("Hint must have at least {0} entries.", EntryLength));
            enumVar = hint[EnumVarIndex].Text;
            castAddress = int.Parse(hint[CastAddressIndex].Text);
            getCurrentAddress = int.Parse(hint[GetCurrentAddressIndex].Text);
            moveNextAddress = int.Parse(hint[MoveNextAddressIndex].Text);
            disposeAddress = int.Parse(hint[DisposeAddressIndex].Text);
        }

        public static ForeachHint FromMetaEntry(MetaEntry[] entry)
        {
            return new ForeachHint(entry);
        }

        public MetaEntry ToMetaEntry()
        {
            MetaEntry[] hint = new MetaEntry[EntryLength];
            hint[0] = Key;
            hint[EnumVarIndex] = EnumVar;
            hint[CastAddressIndex] = CastAddress.ToString();
            hint[GetCurrentAddressIndex] = GetCurrentAddress.ToString();
            hint[MoveNextAddressIndex] = MoveNextAddress.ToString();
            hint[DisposeAddressIndex] = DisposeAddress.ToString();
            return (MetaEntry) hint;
        }

        #endregion

        public string EnumVar
        {
            get
            {
                return enumVar;
            }
        }

        public int CastAddress
        {
            get
            {
                return castAddress;
            }
        }

        public int GetCurrentAddress
        {
            get
            {
                return getCurrentAddress;
            }
        }

        public int MoveNextAddress
        {
            get
            {
                return moveNextAddress;
            }
        }

        public int DisposeAddress
        {
            get
            {
                return disposeAddress;

            }
        }

        #region Methodinfos

        internal static readonly MethodInfo MoveNextMethod = typeof(IEnumerator).GetMethod("MoveNext");
        internal static readonly MethodInfo GetCurrentMethod = typeof(IEnumerator<PValue>).GetProperty("Current").GetGetMethod();
        internal static readonly MethodInfo DisposeMethod = typeof(IDisposable).GetMethod("Dispose");

        #endregion
    }
}