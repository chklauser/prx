// /*
//  * Prexonite, a scripting engine (Scripting Language -> Bytecode -> Virtual Machine)
//  *  Copyright (C) 2007  Christian "SealedSun" Klauser
//  *  E-mail  sealedsun a.t gmail d.ot com
//  *  Web     http://www.sealedsun.ch/
//  *
//  *  This program is free software; you can redistribute it and/or modify
//  *  it under the terms of the GNU General Public License as published by
//  *  the Free Software Foundation; either version 2 of the License, or
//  *  (at your option) any later version.
//  *
//  *  Please contact me (sealedsun a.t gmail do.t com) if you need a different license.
//  * 
//  *  This program is distributed in the hope that it will be useful,
//  *  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  *  GNU General Public License for more details.
//  *
//  *  You should have received a copy of the GNU General Public License along
//  *  with this program; if not, write to the Free Software Foundation, Inc.,
//  *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
//  */

#region Namespace Imports

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

#endregion

namespace Prexonite.Compiler.Cil
{
    public class ForeachHint : ICilHint
    {
        private readonly int _castAddress;
        private readonly int _disposeAddress;
        private readonly string _enumVar;
        private readonly int _getCurrentAddress;
        private readonly int _moveNextAddress;

        public ForeachHint
            (
            string enumVar, int castAddress, int getCurrentAddress, int moveNextAddress, int disposeAddress)
        {
            this._enumVar = enumVar;
            this._disposeAddress = disposeAddress;
            this._moveNextAddress = moveNextAddress;
            this._getCurrentAddress = getCurrentAddress;
            this._castAddress = castAddress;
        }

        #region Meta format

        public const int CastAddressIndex = 1;
        public const int DisposeAddressIndex = 4;
        public const int EntryLength = 5;
        public const int EnumVarIndex = 0;
        public const int GetCurrentAddressIndex = 2;
        public const int MoveNextAddressIndex = 3;
        public const string Key = "foreach";

        public ForeachHint(MetaEntry[] hint)
        {
            if (hint == null)
                throw new ArgumentNullException("hint");
            if (hint.Length < EntryLength)
                throw new ArgumentException(string.Format("Hint must have at least {0} entries.", EntryLength));
            _enumVar = hint[EnumVarIndex + 1].Text;
            _castAddress = int.Parse(hint[CastAddressIndex + 1].Text);
            _getCurrentAddress = int.Parse(hint[GetCurrentAddressIndex + 1].Text);
            _moveNextAddress = int.Parse(hint[MoveNextAddressIndex + 1].Text);
            _disposeAddress = int.Parse(hint[DisposeAddressIndex + 1].Text);
        }

        public static ForeachHint FromMetaEntry(MetaEntry[] entry)
        {
            return new ForeachHint(entry);
        }

        public MetaEntry[] GetFields()
        {
            var fields = new MetaEntry[EntryLength];
            fields[EnumVarIndex] = EnumVar;
            fields[CastAddressIndex] = CastAddress.ToString();
            fields[GetCurrentAddressIndex] = GetCurrentAddress.ToString();
            fields[MoveNextAddressIndex] = MoveNextAddress.ToString();
            fields[DisposeAddressIndex] = DisposeAddress.ToString();
            return fields;
        }

        public String CilKey
        {
            get
            {
                return Key;
            }
        }

        #endregion

        public string EnumVar
        {
            get { return _enumVar; }
        }

        public int CastAddress
        {
            get { return _castAddress; }
        }

        public int GetCurrentAddress
        {
            get { return _getCurrentAddress; }
        }

        public int MoveNextAddress
        {
            get { return _moveNextAddress; }
        }

        public int DisposeAddress
        {
            get { return _disposeAddress; }
        }

        #region Methodinfos

        internal static readonly MethodInfo MoveNextMethod = typeof(IEnumerator).GetMethod("MoveNext");

        internal static readonly MethodInfo GetCurrentMethod =
            typeof(IEnumerator<PValue>).GetProperty("Current").GetGetMethod();

        internal static readonly MethodInfo DisposeMethod = typeof(IDisposable).GetMethod("Dispose");

        #endregion
    }
}