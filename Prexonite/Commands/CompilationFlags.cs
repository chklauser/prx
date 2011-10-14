// Prexonite
// 
// Copyright (c) 2011, Christian Klauser
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
// 
//     Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//     Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//     The names of the contributors may be used to endorse or promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;

namespace Prexonite
{
    /// <summary>
    ///     A bit field that indicates how a subject integrates with cil compilation (compatibility etc.).
    /// </summary>
    [Flags]
    public enum CompilationFlags
    {
        /// <summary>
        ///     Indicates that the subject is fully compatible with compilation to cil.
        /// </summary>
        IsCompatible = 0,

        /// <summary>
        ///     Indicates that the subject cannot be used from compiled functions without special handling.
        /// </summary>
        IsIncompatible = 1,

        /// <summary>
        ///     Indicates that the subject provides a custom compilation routine, invoked via <see
        ///      cref = "ICilCompilerAware.ImplementInCil" />.
        /// </summary>
        HasCustomImplementation = 2,

        /// <summary>
        ///     Indicates that the subject has a static member RunStatically(StackContext, PValue[]) the compiled function could statically bind to.
        /// </summary>
        HasRunStatically = 4,

        /// <summary>
        ///     Indicates that the subject uses dynamic features and requires the caller to be interpreted.
        /// </summary>
        IsDynamic = 8,

        //Shortcuts
        /// <summary>
        ///     Composed. Indicates that the subject is compatible and provides a static method for early binding.
        /// </summary>
        PrefersRunStatically = IsCompatible | HasRunStatically,

        /// <summary>
        ///     Composed. Indicates that the subject is compatible but provides a more efficient custom implementation via <see
        ///      cref = "ICilCompilerAware.ImplementInCil" />.
        /// </summary>
        PrefersCustomImplementation = IsCompatible | HasCustomImplementation,

        /// <summary>
        ///     Composed. Indicates that the subject is not compatible but provides a workaround via <see
        ///      cref = "ICilCompilerAware.ImplementInCil" />.
        /// </summary>
        RequiresCustomImplementation = IsIncompatible | HasCustomImplementation,

        /// <summary>
        ///     Composed. Indicates that the function uses dynamic features (requires an interpreted caller) but apart from that is compatible to cil compilation.
        /// </summary>
        OperatesOnCaller = IsCompatible | IsDynamic
    }
}