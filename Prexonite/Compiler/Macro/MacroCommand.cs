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
using System.Diagnostics;

namespace Prexonite.Compiler.Macro
{
    /// <summary>
    ///     Interface for commands that are applied at compile-time.
    /// </summary>
    public abstract class MacroCommand
    {
        private readonly string _id;

        /// <summary>
        ///     Creates a new instance of the macro command. It will identify itself with the supplied id.
        /// </summary>
        /// <param name = "id">The name of the physical slot, this command resides in.</param>
        protected MacroCommand(string id)
        {
            if (String.IsNullOrWhiteSpace(id))
                throw new ArgumentException("MacroCommad.Id must not be null or empty.");
            _id = id;
        }

        /// <summary>
        ///     ID (slot name) of this macro command.
        /// </summary>
        public string Id
        {
            [DebuggerStepThrough]
            get { return _id; }
        }

        /// <summary>
        ///     Implementation of the application of this macro.
        /// </summary>
        /// <param name = "context">The macro context for this macro expansion.</param>
        protected abstract void DoExpand(MacroContext context);

        /// <summary>
        ///     Expands the macro according to the supplied macro context.
        /// </summary>
        /// <param name = "context">Supplies call site information to the macro.</param>
        public void Expand(MacroContext context)
        {
            DoExpand(context);
        }
    }
}