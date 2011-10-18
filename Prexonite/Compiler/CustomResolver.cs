// Prexonite
// 
// Copyright (c) 2011, Christian Klauser
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
using Prexonite.Compiler.Ast;
using Prexonite.Types;

namespace Prexonite.Compiler
{
    /// <summary>
    ///     A managed symbol resolver.
    /// </summary>
    /// <param name = "t">The compiler target for which to compile code.</param>
    /// <param name = "unresolved">The unresolved AST node.</param>
    /// <returns>Null if no solution could be found. A compatibe node otherwise.</returns>
    public delegate IAstExpression ResolveSymbol(CompilerTarget t, AstUnresolved unresolved);

    /// <summary>
    ///     Encapsulates a user provided resolver.
    /// </summary>
    public sealed class CustomResolver
    {
        private readonly ResolveSymbol _managed;
        private readonly PValue _interpreted;

        /// <summary>
        ///     Creates a new CustomResolver from managed code.
        /// </summary>
        /// <param name = "managedResolver">The implementation of the managed resolver.</param>
        public CustomResolver(ResolveSymbol managedResolver)
        {
            _managed = managedResolver;
        }

        /// <summary>
        ///     Creates a new CustomResolver from interpreted code.
        /// </summary>
        /// <param name = "interpretedResolver">The implementation of the interpreted resolver.</param>
        public CustomResolver(PValue interpretedResolver)
        {
            _interpreted = interpretedResolver;
        }

        /// <summary>
        ///     Determines if the implementation of the resolver is managed code.
        /// </summary>
        public bool IsManaged
        {
            get { return _managed != null; }
        }

        /// <summary>
        ///     Determines if the implementation of the resolver is interpreted code.
        /// </summary>
        public bool IsInterpreted
        {
            get { return _interpreted != null; }
        }

        /// <summary>
        ///     Applies the encapsulated custom resolver to the supplied AST node.
        /// </summary>
        /// <param name = "t">The compiler target for which to resolve the node.</param>
        /// <param name = "unresolved">The unresolved AST node.</param>
        /// <returns>Null if no solution has been found. A compatible AST node otherwise.</returns>
        public IAstExpression Resolve(CompilerTarget t, AstUnresolved unresolved)
        {
            if (IsManaged)
            {
                return _managed(t, unresolved);
            }
            else if (IsInterpreted)
            {
                var presult = _interpreted.IndirectCall
                    (
                        t.Loader, new[]
                            {
                                t.Loader.CreateNativePValue(t),
                                t.Loader.CreateNativePValue(unresolved)
                            });
                if (presult.Type is ObjectPType)
                    return (IAstExpression) presult.Value;
                else
                    return null;
            }
            else
            {
                throw new InvalidOperationException(
                    "Invalid custom resolver. No implementation provided.");
            }
        }
    }
}