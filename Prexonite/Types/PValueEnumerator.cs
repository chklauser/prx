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

using System.Collections;

namespace Prexonite.Types;

public abstract class PValueEnumerator : IEnumerator<PValue>, IObject
{
    /// <summary>
    ///     Returns the current element
    /// </summary>
    public abstract PValue Current { get; }

    /// <summary>
    ///     Returns the current PValue (as an object)
    /// </summary>
    object IEnumerator.Current => Current;

    /// <summary>
    ///     Releases all managed and unmanaged resources held by this instance.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected abstract void Dispose(bool disposing);

    /// <summary>
    ///     Moves on to the next value.
    /// </summary>
    /// <returns>True if that next value exists; false otherwise.</returns>
    public abstract bool MoveNext();

    /// <summary>
    ///     Resets the base enumerator.
    /// </summary>
    /// <remarks>
    ///     Some enumerators may not support the <see cref = "IEnumerator.Reset" /> method.
    /// </remarks>
    /// <exception cref = "NotSupportedException">The base enumerator does not support resetting.</exception>
    public virtual void Reset()
    {
        throw new NotSupportedException(GetType().Name +
            " does not support System.Collections.IEnumerator.Reset()");
    }

    /// <summary>
    ///     Dynamically calls members of <see cref = "PValueEnumeratorWrapper" />.
    /// </summary>
    /// <param name = "sctx">The stack context in which to call the memeber.</param>
    /// <param name = "args">The array of arguments to be passed to the member call.</param>
    /// <param name = "call">The call method. (ignored)</param>
    /// <param name = "id">The name of the member to call.</param>
    /// <param name = "result">The PValue returned by the member call.</param>
    /// <returns>True if the call was successful; false otherwise.</returns>
    /// <remarks>
    ///     Since none of the instance members take any arguments, <paramref name = "args" /> must have length 0.
    /// </remarks>
    public bool TryDynamicCall(
        StackContext sctx, PValue[] args, PCall call, string id, [NotNullWhen(true)] out PValue? result)
    {
        result = null;
        if (args == null)
            throw new ArgumentNullException(nameof(args));
        if (args.Length != 0)
            return false;

        switch (id.ToLower())
        {
            case "movenext":
                result = MoveNext();
                break;
            case "current":
                result = Current;
                break;
            case "reset":
                Reset();
                result = PType.Null.CreatePValue();
                break;
            case "dispose":
                Dispose();
                result = PType.Null.CreatePValue();
                break;
        }

        return result != null;
    }
}