/*
 * Prx, a standalone command line interface to the Prexonite scripting engine.
 * Prexonite, a scripting engine (Scripting Language -> Bytecode -> Virtual Machine)
 *  Copyright (C) 2007  Christian "SealedSun" Klauser
 *  E-mail  sealedsun a.t gmail d.ot com
 *  Web     http://www.sealedsun.ch/
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  Please contact me (sealedsun a.t gmail do.t com) if you need a different license.
 * 
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

using System;
using Prexonite.Compiler.Ast;
using Prexonite.Types;

namespace Prexonite.Compiler
{
    /// <summary>
    /// A managed symbol resolver.
    /// </summary>
    /// <param name="t">The compiler target for which to compile code.</param>
    /// <param name="unresolved">The unresolved AST node.</param>
    /// <returns>Null if no solution could be found. A compatibe node otherwise.</returns>
    public delegate IAstExpression ResolveSymbol(CompilerTarget t, AstUnresolved unresolved);

    /// <summary>
    /// Encapsulates a user provided resolver. 
    /// </summary>
    public sealed class CustomResolver
    {
        private readonly ResolveSymbol _managed;
        private readonly PValue _interpreted;

        /// <summary>
        /// Creates a new CustomResolver from managed code.
        /// </summary>
        /// <param name="managedResolver">The implementation of the managed resolver.</param>
        public CustomResolver(ResolveSymbol managedResolver)
        {
            _managed = managedResolver;
        }

        /// <summary>
        /// Creates a new CustomResolver from interpreted code.
        /// </summary>
        /// <param name="interpretedResolver">The implementation of the interpreted resolver.</param>
        public CustomResolver(PValue interpretedResolver)
        {
            _interpreted = interpretedResolver;
        }

        /// <summary>
        /// Determines if the implementation of the resolver is managed code.
        /// </summary>
        public bool IsManaged
        {
            get
            {
                return _managed != null;
            }
        }

        /// <summary>
        /// Determines if the implementation of the resolver is interpreted code.
        /// </summary>
        public bool IsInterpreted
        {
            get
            {
                return _interpreted != null;
            }
        }

        /// <summary>
        /// Applies the encapsulated custom resolver to the supplied AST node.
        /// </summary>
        /// <param name="t">The compiler target for which to resolve the node.</param>
        /// <param name="unresolved">The unresolved AST node.</param>
        /// <returns>Null if no solution has been found. A compatible AST node otherwise.</returns>
        public IAstExpression Resolve(CompilerTarget t, AstUnresolved unresolved)
        {
            if(IsManaged)
            {
                return _managed(t, unresolved);
            }
            else if(IsInterpreted)
            {
                var presult = _interpreted.IndirectCall
                    (t.Loader, new[]
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
                throw new InvalidOperationException("Invalid custom resolver. No implementation provided."); 
            }
        }
    }
}
