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
using Prexonite.Types;

namespace Prexonite.Commands
{
    /// <summary>
    /// Command that calls <see cref="IDisposable.Dispose"/> on object values that support the interface.
    /// </summary>
    /// <remarks>
    /// Note that only wrapped .NET objects are disposed. Custom types that respond to "Dispose" are ignored.
    /// </remarks>
    public class Dispose : PCommand
    {
        public const string DisposeMemberId = "Dispose";

        /// <summary>
        /// Executes the dispose function.<br />
        /// Calls <see cref="IDisposable.Dispose"/> on object values that support the interface.
        /// </summary>
        /// <param name="sctx">The stack context. Ignored by this command.</param>
        /// <param name="args">The list of values to dispose.</param>
        /// <returns>Always null.</returns>
        /// <remarks><para>
        /// Note that only wrapped .NET objects are disposed. Custom types that respond to "Dispose" are ignored.</para>
        /// </remarks>
        public override PValue Run(StackContext sctx, PValue[] args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            foreach (PValue arg in args)
                if (arg != null)
                {
                    PValue dummy;
                    if (arg.Type is ObjectPType)
                    {
                        IDisposable toDispose = arg.Value as IDisposable;
                        if (toDispose != null)
                            toDispose.Dispose();
                        else
                        {
                            IObject isObj = arg.Value as IObject;
                            if (isObj != null)
                            {
                                isObj.TryDynamicCall(
                                    sctx, new PValue[0], PCall.Get, DisposeMemberId, out dummy);
                            }
                        }
                    }
                    else
                    {
                        arg.TryDynamicCall(sctx, new PValue[0], PCall.Get, DisposeMemberId, out dummy);
                    }
                }
            return PType.Null.CreatePValue();
        }

        /// <summary>
        /// A flag indicating whether the command acts like a pure function.
        /// </summary>
        /// <remarks>Pure commands can be applied at compile time.</remarks>
        public override bool IsPure
        {
            get { return false; }
        }
    }
}