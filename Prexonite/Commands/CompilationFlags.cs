/*
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
using System.Collections.Generic;
using System.Text;

namespace Prexonite
{
    /// <summary>
    /// A bit field that indicates how a subject integrates with cil compilation (compatibility etc.).
    /// </summary>
    [Flags]
    public enum CompilationFlags
    {
        /// <summary>
        /// Indicates that the subject is fully compatible with compilation to cil.
        /// </summary>
        IsCompatible = 0,

        /// <summary>
        /// Indicates that the subject cannot be used from compiled functions without special handling.
        /// </summary>
        IsIncompatible = 1,

        /// <summary>
        /// Indicates that the subject provides a custom compilation routine, invoked via <see cref="ICilCompilerAware.ImplementInCil"/>.
        /// </summary>
        HasCustomImplementation = 2,

        /// <summary>
        /// Indicates that the subject has a static member RunStatically(StackContext, PValue[]) the compiled function could statically bind to.
        /// </summary>
        HasRunStatically = 4,

        /// <summary>
        /// Indicates that the subject uses dynamic features and requires the caller to be interpreted.
        /// </summary>
        IsDynamic = 8,

        //Shortcuts
        /// <summary>
        /// Composed. Indicates that the subject is compatible and provides a static method for early binding.
        /// </summary>
        PrefersRunStatically = IsCompatible | HasRunStatically,

        /// <summary>
        /// Composed. Indicates that the subject is compatible but provides a more efficient custom implementation via <see cref="ICilCompilerAware.ImplementInCil"/>.
        /// </summary>
        PrefersCustomImplementation = IsCompatible | HasCustomImplementation,

        /// <summary>
        /// Composed. Indicates that the subject is not compatible but provides a workaround via <see cref="ICilCompilerAware.ImplementInCil"/>.
        /// </summary>
        RequiresCustomImplementation = IsIncompatible | HasCustomImplementation,

        /// <summary>
        /// Composed. Indicates that the function uses dynamic features (requires an interpreted caller) but apart from that is compatible to cil compilation.
        /// </summary>
        OperatesOnCaller = IsCompatible | IsDynamic
    }
}