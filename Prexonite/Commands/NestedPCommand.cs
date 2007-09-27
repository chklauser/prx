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

namespace Prexonite.Commands
{
    /// <summary>
    /// Implementation of <see cref="PCommand"/> that forwards the run call to 
    /// a class that implements <see cref="ICommand"/>.
    /// </summary>
    /// <seealso cref="PCommand"/>
    /// <seealso cref="ICommand"/>
    public sealed class NestedPCommand : PCommand
    {
        private ICommand _action;

        /// <summary>
        /// Provides access to the implementation of this specific instance of <see cref="NestedPCommand"/>.
        /// </summary>
        public ICommand Action
        {
            get { return _action; }
        }

        /// <summary>
        /// Creates a new <see cref="NestedPCommand"/>.
        /// </summary>
        /// <param name="action">Any implementation of <see cref="ICommand"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="action"/> is null.</exception>
        public NestedPCommand(ICommand action)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            _action = action;
        }

        /// <summary>
        /// Executes <see cref="ICommand.Run"/> on <see cref="Action"/>.
        /// </summary>
        /// <param name="sctx">The stack context in which to execute the command.</param>
        /// <param name="args">The arguments to pass to the command invocation.</param>
        /// <returns>The value returned by <c><see cref="Action"/>.Run</c>.</returns>
        public override PValue Run(StackContext sctx, PValue[] args)
        {
            return _action.Run(sctx, args);
        }

        /// <summary>
        /// Returns a description of the nested command instance.
        /// </summary>
        /// <returns>A description of the nested command instance.</returns>
        public override string ToString()
        {
            return "Nested(" + _action + ")";
        }
    }

    /// <summary>
    /// Interface to be implemented by a class to be used as a command.
    /// </summary>
    /// <seealso cref="PCommand"/>
    /// <seealso cref="NestedPCommand"/>
    /// <remarks>In order to be used as a command, <see cref="NestedPCommand"/> need to be wrapped around instances of types that implement this interface.</remarks>
    public interface ICommand
    {
        /// <summary>
        /// Actual implementation of a command.
        /// </summary>
        /// <param name="sctx">The stack context in which the command is executed.</param>
        /// <param name="args">The array of arguments supplied to the command.</param>
        /// <returns>The value returned by the command.</returns>
        /// <remarks>If your implementation does not return a value, you have to return <c>PType.Null.CreatePValue()</c> and <strong>not</strong> <c>null</c>!</remarks>
        PValue Run(StackContext sctx, PValue[] args);
    }
}