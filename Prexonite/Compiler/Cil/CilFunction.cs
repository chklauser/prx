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
namespace Prexonite.Compiler.Cil
{
    /// <summary>
    /// A managed implementation of a <see cref="PFunction"/>s byte code.
    /// </summary>
    /// <param name="source">A reference to the original function. (The source code)</param>
    /// <param name="sctx">The stack context used by the calling function.</param>
    /// <param name="args">An array of arguments. Must not be null, but may be empty.</param>
    /// <param name="sharedVariables">An array of variables shared with the callee. May be null if no variables are shared.</param>
    /// <param name="returnValue">Will hold the value returned by the function. Will never be a null reference.</param>
    /// <param name="returnMode">Will hold the return mode chosen by the function.</param>
    public delegate void CilFunction(
        PFunction source, StackContext sctx, PValue[] args, PVariable[] sharedVariables, out PValue returnValue, out ReturnMode returnMode);
}