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
#region

using System;
using System.Collections;
using System.Collections.Generic;

#endregion

namespace Prexonite.Types;

/// <summary>
///     An enumerator proxy that returns the values instead of PValue objects of an <see cref = "IEnumerable{T}" />
/// </summary>
public sealed class PValueEnumeratorWrapper : PValueEnumerator
{
    #region Class

    private readonly IEnumerator<PValue> _baseEnumerator;

    /// <summary>
    ///     Creates a new proxy for the IEnumerator of the supplied <paramref name = "enumerable" />.
    /// </summary>
    /// <param name = "enumerable">An IEnumerable.</param>
    public PValueEnumeratorWrapper(IEnumerable<PValue> enumerable)
        : this(enumerable.GetEnumerator())
    {
    }

    /// <summary>
    ///     Creates a new prox for the supplied enumerator.
    /// </summary>
    /// <param name = "baseEnumerator">An IEnumerator</param>
    public PValueEnumeratorWrapper(IEnumerator<PValue> baseEnumerator)
    {
        _baseEnumerator = baseEnumerator ?? throw new ArgumentNullException(nameof(baseEnumerator));
    }

    #endregion

    #region IEnumerator<PValue> Members

    /// <summary>
    ///     Returns the current element
    /// </summary>
    public override PValue Current => _baseEnumerator.Current;

    #endregion

    #region IDisposable Members

    // Dispose() calls Dispose(true)

    // The bulk of the clean-up code is implemented in Dispose(bool)
    protected override void Dispose(bool disposing)
    {
        if (!disposing)
            return;
        // free managed resources 
        _baseEnumerator?.Dispose();
    }

    #endregion

    #region IEnumerator Members

    /// <summary>
    ///     Moves on to the next value.
    /// </summary>
    /// <returns>True if that next value exists; false otherwise.</returns>
    public override bool MoveNext()
    {
        return _baseEnumerator.MoveNext();
    }

    /// <summary>
    ///     Resets the base enumerator.
    /// </summary>
    /// <remarks>
    ///     Some enumerators may not support the <see cref = "IEnumerator.Reset" /> method.
    /// </remarks>
    /// <exception cref = "NotSupportedException">The base enumerator does not support resetting.</exception>
    public override void Reset()
    {
        _baseEnumerator.Reset();
    }

    #endregion

    #region IObject Members

    #endregion
}