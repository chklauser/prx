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
using System.Diagnostics;

namespace Prexonite.Compiler
{
    /// <summary>
    /// A method that modifies the supplied 
    /// <see cref="CompilerTarget"/> when invoked prior to optimization and code generation.
    /// </summary>
    /// <param name="target">The <see cref="CompilerTarget"/> of the function to be modified.</param>
    public delegate void AstTransformation(CompilerTarget target);

    /// <summary>
    /// Union class for both managed as well as interpreted compiler hooks.
    /// </summary>
    [DebuggerNonUserCode]
    public sealed class CompilerHook
    {
        private readonly AstTransformation _managed;
        private readonly PValue _interpreted;

        /// <summary>
        /// Creates a new compiler hook, that executes a managed method.
        /// </summary>
        /// <param name="transformation">A managed transformation.</param>
        public CompilerHook(AstTransformation transformation)
        {
            if (transformation == null)
                throw new ArgumentNullException("transformation");
            _managed = transformation;
        }

        /// <summary>
        /// Creates a new compiler hook, that indirectly calls a <see cref="PValue"/>.
        /// </summary>
        /// <param name="transformation">A value that supports indirect calls (such as a function reference).</param>
        public CompilerHook(PValue transformation)
        {
            if (transformation == null)
                throw new ArgumentNullException("transformation");
            _interpreted = transformation;
        }

        /// <summary>
        /// Indicates whether the compiler hook is managed.
        /// </summary>
        public bool IsManaged
        {
            get { return _managed != null; }
        }

        /// <summary>
        /// Indicates whether the compiler hook is interpreted.
        /// </summary>
        public bool IsInterpreted
        {
            get { return _interpreted != null; }
        }

        /// <summary>
        /// Executes the compiler hook (either calls the managed 
        /// delegate or indirectly calls the <see cref="PValue"/> in the context of the <see cref="Loader"/>.)
        /// </summary>
        /// <param name="target">The compiler target to modify.</param>
        public void Execute(CompilerTarget target)
        {
            try
            {
                target.Loader.Options.TargetApplication._SuppressInitialization = true;
                if (IsManaged)
                    _managed(target);
                else
                    _interpreted.IndirectCall(
                        target.Loader, new PValue[] {target.Loader.CreateNativePValue(target)});
            }
            finally
            {
                target.Loader.Options.TargetApplication._SuppressInitialization = false;
            }
        }
    }
}