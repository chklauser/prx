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
using System.Diagnostics.CodeAnalysis;
using Prexonite.Compiler.Cil;

namespace Prexonite.Commands
{
    /// <summary>
    ///     <para>An interface implemented by elements that require special treatment for CIL compilation.</para>
    ///     <para>CIL extensions have access to compile-time constant arguments and can provide customized CIL code as their implementation.</para>
    ///     <para>The implementation of this interface does not affect execution under the Prexonite VM.</para>
    ///     <para>Currently only commands are checked for CIL extensions.</para>
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = "Cil")]
    public interface ICilExtension
    {
        /// <summary>
        ///     Checks whether the static arguments and number of dynamic arguments are valid for the CIL extension. 
        /// 
        ///     <para>Returning false means that the CIL extension cannot provide a CIL implementation for the set of arguments at hand. In that case the CIL compiler will fall back to  <see
        ///       cref = "ICilCompilerAware" /> and finally the built-in mechanisms.</para>
        ///     <para>Returning true means that the CIL extension can provide a CIL implementation for the set of arguments at hand. In that case the CIL compiler may subsequently call <see
        ///      cref = "Implement" /> with the same set of arguments.</para>
        /// </summary>
        /// <param name = "staticArgv">The suffix of compile-time constant arguments, starting after the last dynamic (not compile-time constant) argument. An empty array means that there were no compile-time constant arguments at the end.</param>
        /// <param name = "dynamicArgc">The number of dynamic arguments preceding the supplied static arguments. The total number of arguments is determined by <code>(staticArgv.Length + dynamicArgc)</code></param>
        /// <returns>true if the extension can provide a CIL implementation for the set of arguments; false otherwise</returns>
        bool ValidateArguments(CompileTimeValue[] staticArgv, int dynamicArgc);

        /// <summary>
        ///     Implements the CIL extension in CIL for the supplied arguments. The CIL compiler guarantees to always first call <see
        ///      cref = "ValidateArguments" /> in order to establish whether the extension can actually implement a particular call.
        ///     Thus, this method does not have to verify <paramref name = "staticArgv" /> and <paramref name = "dynamicArgc" />.
        /// </summary>
        /// <param name = "state">The CIL compiler state. This object is used to emit instructions.</param>
        /// <param name = "ins">The instruction that "calls" the CIL extension. Usually a command call.</param>
        /// <param name = "staticArgv">The suffix of compile-time constant arguments, starting after the last dynamic (not compile-time constant) argument. An empty array means that there were no compile-time constant arguments at the end.</param>
        /// <param name = "dynamicArgc">The number of dynamic arguments preceding the supplied static arguments. The total number of arguments is determined by <code>(staticArgv.Length + dynamicArgc)</code></param>
        void Implement(CompilerState state, Instruction ins, CompileTimeValue[] staticArgv,
            int dynamicArgc);
    }
}