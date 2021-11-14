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
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Prexonite.Compiler.Cil;

/// <summary>
///     Wraps a cil extension hint. Should replace existing hints of the same kind.
/// </summary>
[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
    MessageId = "Cil")]
public class CilExtensionHint : ICilHint
{
    /// <summary>
    ///     The CIL hint key for CIL extensions.
    /// </summary>
    public const string Key = "ext";

    /// <summary>
    ///     The offsets at which CIL extension code begins.
    /// </summary>
    public IList<int> Offsets { [DebuggerStepThrough] get; }

    /// <summary>
    ///     Creates a new CIL extension hint.
    /// </summary>
    /// <param name = "offsets">The offsets at which CIL extension code begins</param>
    public CilExtensionHint(IList<int> offsets)
    {
        Offsets = offsets ?? throw new ArgumentNullException(nameof(offsets));
    }

    #region Implementation of ICilHint

    /// <summary>
    ///     The key under which this CIL hint is stored. This key is used to deserialize the hint into the correct format.
    /// </summary>
    public string CilKey => Key;

    /// <summary>
    ///     Get the list of fields to be serialized. Does not include the key.
    /// </summary>
    /// <returns>The list of fields to be serialized.</returns>
    public MetaEntry[] GetFields()
    {
        return (from address in Offsets
            select (MetaEntry) address.ToString()).ToArray();
    }

    #endregion

    /// <summary>
    ///     Parses CIL extension hint from a meta entry.
    /// </summary>
    /// <param name = "hint"></param>
    /// <returns></returns>
    public static CilExtensionHint FromMetaEntry(MetaEntry[] hint)
    {
        var offsets = new List<int>(hint.Length);
        foreach (var metaEntry in hint.Skip(1))
        {
            if (metaEntry == null)
                continue;
            if (int.TryParse(metaEntry.Text, out var offset))
                offsets.Add(offset);
        }

        return new CilExtensionHint(offsets);
    }
}