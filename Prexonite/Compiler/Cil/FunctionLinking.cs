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

using System;

#endregion

namespace Prexonite.Compiler.Cil
{
    [Flags]
    public enum FunctionLinking
    {
        /// <summary>
        ///     The CIL implementation itself is not available for static linking.
        /// </summary>
        Isolated = 0,

        /// <summary>
        ///     The CIL implementation is available for static linking
        /// </summary>
        AvailableForLinking = 1,

        /// <summary>
        ///     Function calls are always linked by name.
        /// </summary>
        ByName = 0,

        /// <summary>
        ///     Function calls are linked statically whenever possible.
        /// </summary>
        Static = 2,

        /// <summary>
        ///     The CIL implementation is completely independent of other implementations.
        /// </summary>
        FullyIsolated = Isolated | ByName,

        /// <summary>
        ///     The CIL implementation supports static linking wherever possible.
        /// </summary>
        FullyStatic = AvailableForLinking | Static,

        /// <summary>
        ///     The CIL implementation is isolated but links statically to functions available for linking.
        /// </summary>
        JustStatic = Isolated | Static,

        /// <summary>
        ///     The CIL implementation is available for static linking but links just by name.
        /// </summary>
        JustAvailableForLinking = AvailableForLinking | ByName
    }
}