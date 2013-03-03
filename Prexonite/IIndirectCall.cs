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
using System;
using JetBrains.Annotations;

namespace Prexonite
{
    /// <summary>
    ///     Classes implementing this interface can react to indirect calls from Prexonite Script Code.
    /// </summary>
    /// <example>
    ///     <code>function main()
    ///         {
    ///         var obj = Get_an_object_that_implements_IIndirectCall();
    ///         obj.("argument"); //<see cref = "IndirectCall" /> will be called with the supplied argument.
    ///         }</code>
    /// </example>
    public interface IIndirectCall
    {
        /// <summary>
        ///     The reaction to an indirect call.
        /// </summary>
        /// <param name = "sctx">The stack context in which the object has been called indirectly.</param>
        /// <param name = "args">The array of arguments passed to the call.</param>
        /// <remarks>
        ///     <para>
        ///         Neither <paramref name = "sctx" /> nor <paramref name = "args" /> should be null. 
        ///         Implementations should raise an <see cref = "ArgumentNullException" /> when confronted with null as the StackContext.<br />
        ///         A null reference as the argument array should be silently converted to an empty array.
        ///     </para>
        ///     <para>
        ///         Implementations should <b>never</b> return null but instead return a <see cref = "PValue" /> object containing null.
        ///         <code>return Prexonite.Types.PType.Null.CreatePValue();</code>
        ///     </para>
        /// </remarks>
        /// <returns>The result of the call. Should <strong>never</strong> be null.</returns>
        [NotNull]
        PValue IndirectCall([NotNull] StackContext sctx, [NotNull] PValue[] args);
    }
}