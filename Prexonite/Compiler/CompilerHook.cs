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
using System.Diagnostics;

namespace Prexonite.Compiler
{
    /// <summary>
    ///     A method that modifies the supplied 
    ///     <see cref = "CompilerTarget" /> when invoked prior to optimization and code generation.
    /// </summary>
    /// <param name = "target">The <see cref = "CompilerTarget" /> of the function to be modified.</param>
    public delegate void AstTransformation(CompilerTarget target);

    /// <summary>
    ///     Union class for both managed as well as interpreted compiler hooks.
    /// </summary>
    [DebuggerNonUserCode]
    public sealed class CompilerHook
    {
        private readonly AstTransformation _managed;
        private readonly PValue _interpreted;

        /// <summary>
        ///     Creates a new compiler hook, that executes a managed method.
        /// </summary>
        /// <param name = "transformation">A managed transformation.</param>
        public CompilerHook(AstTransformation transformation)
        {
            if (transformation == null)
                throw new ArgumentNullException("transformation");
            _managed = transformation;
        }

        /// <summary>
        ///     Creates a new compiler hook, that indirectly calls a <see cref = "PValue" />.
        /// </summary>
        /// <param name = "transformation">A value that supports indirect calls (such as a function reference).</param>
        public CompilerHook(PValue transformation)
        {
            if (transformation == null)
                throw new ArgumentNullException("transformation");
            _interpreted = transformation;
        }

        /// <summary>
        ///     Indicates whether the compiler hook is managed.
        /// </summary>
        public bool IsManaged
        {
            get { return _managed != null; }
        }

        /// <summary>
        ///     Indicates whether the compiler hook is interpreted.
        /// </summary>
        public bool IsInterpreted
        {
            get { return _interpreted != null; }
        }

        /// <summary>
        ///     Executes the compiler hook (either calls the managed 
        ///     delegate or indirectly calls the <see cref = "PValue" /> in the context of the <see cref = "Loader" />.)
        /// </summary>
        /// <param name = "target">The compiler target to modify.</param>
        public void Execute(CompilerTarget target)
        {
            try
            {
                target.Loader.Options.TargetApplication._SuppressInitialization = true;
                if (IsManaged)
                    _managed(target);
                else
                    _interpreted.IndirectCall(
                        target.Loader, new[] {target.Loader.CreateNativePValue(target)});
            }
            finally
            {
                target.Loader.Options.TargetApplication._SuppressInitialization = false;
            }
        }
    }
}