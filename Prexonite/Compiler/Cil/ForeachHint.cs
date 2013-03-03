// Prexonite
// 
// Copyright (c) 2013, Christian Klauser
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
            string enumVar, int castAddress, int getCurrentAddress, int moveNextAddress,
            int disposeAddress)
        {
            _enumVar = enumVar;
            _disposeAddress = disposeAddress;
            _moveNextAddress = moveNextAddress;
            _getCurrentAddress = getCurrentAddress;
            _castAddress = castAddress;
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
                throw new ArgumentException(string.Format("Hint must have at least {0} entries.",
                    EntryLength));
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
            get { return Key; }
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

        internal static readonly MethodInfo MoveNextMethod =
            typeof (IEnumerator).GetMethod("MoveNext");

        internal static readonly MethodInfo GetCurrentMethod =
            typeof (IEnumerator<PValue>).GetProperty("Current").GetGetMethod();

        internal static readonly MethodInfo DisposeMethod = typeof (IDisposable).GetMethod("Dispose");

        #endregion
    }
}