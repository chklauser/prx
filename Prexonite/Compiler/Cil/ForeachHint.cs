// Prexonite
// 
// Copyright (c) 2014, Christian Klauser
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

using System.Collections;
using System.Reflection;

#endregion

namespace Prexonite.Compiler.Cil;

public class ForeachHint : ICilHint
{
    public ForeachHint
    (
        string enumVar, int castAddress, int getCurrentAddress, int moveNextAddress,
        int disposeAddress)
    {
        EnumVar = enumVar;
        DisposeAddress = disposeAddress;
        MoveNextAddress = moveNextAddress;
        GetCurrentAddress = getCurrentAddress;
        CastAddress = castAddress;
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
            throw new ArgumentNullException(nameof(hint));
        if (hint.Length < EntryLength)
            throw new ArgumentException($"Hint must have at least {EntryLength} entries.");
        EnumVar = hint[EnumVarIndex + 1].Text;
        CastAddress = int.Parse(hint[CastAddressIndex + 1].Text);
        GetCurrentAddress = int.Parse(hint[GetCurrentAddressIndex + 1].Text);
        MoveNextAddress = int.Parse(hint[MoveNextAddressIndex + 1].Text);
        DisposeAddress = int.Parse(hint[DisposeAddressIndex + 1].Text);
    }

    public static ForeachHint FromMetaEntry(MetaEntry[] entry)
    {
        return new(entry);
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

    public string CilKey => Key;

    #endregion

    public string EnumVar { get; }

    public int CastAddress { get; }

    public int GetCurrentAddress { get; }

    public int MoveNextAddress { get; }

    public int DisposeAddress { get; }

    #region Methodinfos

    internal static readonly MethodInfo MoveNextMethod =
        typeof(IEnumerator).GetMethod("MoveNext")
        ?? throw new InvalidOperationException(
            $"{nameof(IEnumerator)}.{nameof(IEnumerator.MoveNext)} method not found.");

    internal static readonly MethodInfo GetCurrentMethod =
        typeof(IEnumerator<PValue>).GetProperty(nameof(IEnumerator<PValue>.Current))!.GetGetMethod()
        ?? throw new InvalidOperationException(
            $"{nameof(IEnumerator)}.{nameof(IEnumerator.Current)} property getter not found.");

    internal static readonly MethodInfo DisposeMethod = typeof(IDisposable).GetMethod(nameof(IDisposable.Dispose))
        ?? throw new InvalidOperationException(
            $"{nameof(IDisposable)}.{nameof(IDisposable.Dispose)} method not found.");

    #endregion
}