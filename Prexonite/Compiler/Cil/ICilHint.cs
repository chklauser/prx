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
using System;
using System.Diagnostics.CodeAnalysis;

namespace Prexonite.Compiler.Cil;

/// <summary>
///     A cil hint. Can be serialized to a meta entry for storage.
/// </summary>
[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
    MessageId = "Cil")]
public interface ICilHint
{
    /// <summary>
    ///     The key under which this CIL hint is stored. This key is used to deserialize the hint into the correct format.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = "Cil")]
    string CilKey { get; }

    /// <summary>
    ///     Get the list of fields to be serialized. Does not include the key.
    /// </summary>
    /// <returns>The list of fields to be serialized.</returns>
    MetaEntry[] GetFields();
}

[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
    MessageId = "Cil")]
public static class CilHintExtensions
{
    /// <summary>
    ///     Converts the supplied CIL hint to a meta entry (including the CIL hint key).
    /// </summary>
    /// <param name = "hint">The CIL hint to serialize</param>
    /// <returns>The serialized cil hint.</returns>
    public static MetaEntry ToMetaEntry(this ICilHint hint)
    {
        var key = hint.CilKey;
        var fields = hint.GetFields();

        var entry = new MetaEntry[fields.Length + 1];
        entry[0] = key;
        Array.Copy(fields, 0, entry, 1, fields.Length);

        return (MetaEntry) entry;
    }
}