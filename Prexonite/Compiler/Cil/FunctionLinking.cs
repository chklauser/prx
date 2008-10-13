// /*
//  * Prexonite, a scripting engine (Scripting Language -> Bytecode -> Virtual Machine)
//  *  Copyright (C) 2007  Christian "SealedSun" Klauser
//  *  E-mail  sealedsun a.t gmail d.ot com
//  *  Web     http://www.sealedsun.ch/
//  *
//  *  This program is free software; you can redistribute it and/or modify
//  *  it under the terms of the GNU General Public License as published by
//  *  the Free Software Foundation; either version 2 of the License, or
//  *  (at your option) any later version.
//  *
//  *  Please contact me (sealedsun a.t gmail do.t com) if you need a different license.
//  * 
//  *  This program is distributed in the hope that it will be useful,
//  *  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  *  GNU General Public License for more details.
//  *
//  *  You should have received a copy of the GNU General Public License along
//  *  with this program; if not, write to the Free Software Foundation, Inc.,
//  *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
//  */

#region Namespace Imports

using System;

#endregion

namespace Prexonite.Compiler.Cil
{
    [Flags]
    public enum FunctionLinking
    {
        /// <summary>
        /// The CIL implementation itself is not available for static linking.
        /// </summary>
        Isolated = 0,

        /// <summary>
        /// The CIL implementation is available for static linking
        /// </summary>
        AvailableForLinking = 1,

        /// <summary>
        /// Function calls are linked statically whenever possible.
        /// </summary>
        Static = 2,

        /// <summary>
        /// Function calls are always linked by name.
        /// </summary>
        ByName = 0,

        /// <summary>
        /// The CIL implementation is completely independant of other implementations.
        /// </summary>
        FullyIsolated = Isolated | ByName,

        /// <summary>
        /// The CIL implementation supports static linking wherever possible.
        /// </summary>
        FullyStatic = AvailableForLinking | Static,

        /// <summary>
        /// The CIL implementation is isolated but links statically.
        /// </summary>
        JustStatic = Isolated | Static,

        /// <summary>
        /// The CIL implementation is available for static linking but links just by name.
        /// </summary>
        JustAvailableForLinking = AvailableForLinking | ByName
    }
}