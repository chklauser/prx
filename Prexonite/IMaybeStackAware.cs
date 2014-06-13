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
namespace Prexonite
{
    /// <summary>
    ///     Partial application implementations commonly implement this interface. It allows
    ///     entities that are <see cref = "IStackAware" /> to retain this property even if 
    ///     partially applied.
    /// </summary>
    public interface IMaybeStackAware : IIndirectCall
    {
        /// <summary>
        ///     If the particular partial application supports it, create a 
        ///     stack context for executing the application. Otherwise it executes the application.
        /// </summary>
        /// <param name = "sctx">The caller's stack context.</param>
        /// <param name = "args">The arguments passed to the partial application by the caller.</param>
        /// <param name = "partialApplicationContext">If creation of stack context is successful, the stack context for executing 
        ///     the application. Otherwise undefined.</param>
        /// <param name = "result">If the creation of stack context is not successful, the return value of 
        ///     executing the application.</param>
        /// <returns>True if a stack context has been created; false if the application has been executed.</returns>
        bool TryDefer(StackContext sctx, PValue[] args,
            out StackContext partialApplicationContext,
            out PValue result);
    }
}