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
    /// Implementation of <see cref="PCommand"/> using delegates.
    /// </summary>
    /// <seealso cref="PCommand"/>
    /// <seealso cref="PCommandAction"/>
    public sealed class DelegatePCommand : PCommand
    {
        private PCommandAction _action;

        /// <summary>
        /// Provides readonly access to the delegate used to implement the current instance of <see cref="DelegatePCommand"/>.
        /// </summary>
        public PCommandAction Action
        {
            get { return _action; }
        }

        private bool _isPure;

        /// <summary>
        /// A flag indicating whether the command acts like a pure function.
        /// </summary>
        /// <remarks>Pure commands can be applied at compile time.</remarks>
        public override bool IsPure
        {
            get { return _isPure; }
        }

        /// <summary>
        /// Returns a string that describes the current instance of <see cref="DelegatePCommand"/>.
        /// </summary>
        /// <returns>A string that describes the current instance of <see cref="DelegatePCommand"/></returns>
        public override string ToString()
        {
            return "Delegate(" + _action + ")";
        }

        /// <summary>
        /// Forwards the call to the actual implementation, the delegate <see cref="Action"/>.
        /// </summary>
        /// <param name="sctx">The stack context in which to execute the command.</param>
        /// <param name="args">The array of arguments to pass to the command.</param>
        /// <returns></returns>
        public override PValue Run(StackContext sctx, PValue[] args)
        {
            return _action(sctx, args);
        }

        /// <summary>
        /// Creates a new <see cref="DelegatePCommand"/>.
        /// </summary>
        /// <param name="action">An implementation of the <see cref="PCommand.Run"/> method.</param>
        /// <exception cref="ArgumentNullException"><paramref name="action"/> is null.</exception>
        public DelegatePCommand(PCommandAction action)
            : this(action, false)
        {
        }

        /// <summary>
        /// Creates a new <see cref="DelegatePCommand"/>.
        /// </summary>
        /// <param name="action">An implementation of the <see cref="PCommand.Run"/> method.</param>
        /// <param name="isPure">A boolean value indicating whether the command is to be treated like a pure function.</param>
        /// <exception cref="ArgumentNullException"><paramref name="action"/> is null.</exception>
        public DelegatePCommand(PCommandAction action, bool isPure)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            _action = action;
            _isPure = isPure;
        }

        /// <summary>
        /// Syntactic sugar for the creation of commands.
        /// </summary>
        /// <param name="action">An implementation of the <see cref="PCommand.Run"/> method.</param>
        /// <returns>A new instance of <see cref="DelegatePCommand"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="action"/> is null.</exception>
        public static implicit operator DelegatePCommand(PCommandAction action)
        {
            return new DelegatePCommand(action);
        }
    }

    /// <summary>
    /// Emulates <see cref="PCommand.Run"/> for use in <see cref="DelegatePCommand"/>.
    /// </summary>
    /// <param name="sctx">The stack context in which the command is executed.</param>
    /// <param name="arguments">The array of arguments passed to the command invocation.</param>
    /// <returns>The value returned by the command.</returns>
    /// <remarks>If your implementation does not return a value, you have to return <c>PType.Null.CreatePValue()</c> and <strong>not</strong> <c>null</c>!</remarks>
    /// <seealso cref="DelegatePCommand"/>
    /// <seealso cref="PCommand"/>
    public delegate PValue PCommandAction(StackContext sctx, PValue[] arguments);
}