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

namespace Prexonite
{

    /// <summary>
    /// Classes implementing this interface can react to indirect calls from Prexonite Script Code.
    /// </summary>
    /// <example><code>function main()
    /// {
    ///     var obj = Get_an_object_that_implements_IIndirectCall();
    ///     obj.("argument"); //<see cref="IndirectCall"/> will be called with the supplied argument.
    /// }</code></example>
    public interface IIndirectCall
    {
        /// <summary>
        /// The reaction to an indirect call. 
        /// </summary>
        /// <param name="sctx">The stack context in which the object has been called indirectly.</param>
        /// <param name="args">The array of arguments passed to the call.</param>
        /// <remarks>
        ///     <para>
        ///         Neither <paramref name="sctx"/> nor <paramref name="args"/> should be null. 
        /// Implementations should raise an <see cref="ArgumentNullException"/> when confronted with null as the StackContext.<br />
        /// A null reference as the argument array should be silently converted to an empty array.
        ///     </para>
        ///     <para>
        ///         Implementations should <b>never</b> return null but instead return a <see cref="PValue"/> object containing null.
        /// <code>return Prexonite.Types.PType.Null.CreatePValue();</code>
        ///     </para>
        /// </remarks>
        /// <returns>The result of the call. Should <strong>never</strong> be null.</returns>
        PValue IndirectCall(StackContext sctx, PValue[] args);
    }
}