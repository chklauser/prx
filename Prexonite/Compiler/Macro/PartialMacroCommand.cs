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
namespace Prexonite.Compiler.Macro
{
    /// <summary>
    ///     Interface for macro commands that also handle (some) partial applications.
    /// </summary>
    public abstract class PartialMacroCommand : MacroCommand
    {
        /// <summary>
        ///     Creates a new partially applicable macro.
        /// </summary>
        /// <param name = "id">The id of this macro</param>
        protected PartialMacroCommand(string id) : base(id)
        {
        }

        /// <summary>
        ///     Implements the expansion of the partially applied macro. May refuse certain partial applications.
        /// </summary>
        /// <param name = "context">The macro context for this macro expansion.</param>
        /// <returns>True, if the macro was successfully applied partially; false if partial application is illegal in this particular case.</returns>
        protected abstract bool DoExpandPartialApplication(MacroContext context);

        /// <summary>
        ///     Attempts to expand the partial macro application.
        /// </summary>
        /// <param name = "context">The macro context for this macro expansion.</param>
        /// <returns>True, if the macro was successfully applied partially; false if partial application is illegal in this particular case.</returns>
        public bool ExpandPartialApplication(MacroContext context)
        {
            return DoExpandPartialApplication(context);
        }
    }
}