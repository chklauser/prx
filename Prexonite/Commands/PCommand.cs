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
using Prexonite.Types;

namespace Prexonite.Commands
{
    /// <summary>
    /// The abstract base class for all commands (built-in functions)
    /// </summary>
    public abstract class PCommand : IIndirectCall
    {
        /// <summary>
        /// A flag indicating whether the command acts like a pure function.
        /// </summary>
        /// <remarks>Pure commands can be applied at compile time.</remarks>
        public abstract bool IsPure { get; }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="sctx">The stack context in which to execut the command.</param>
        /// <param name="args">The arguments to be passed to the command.</param>
        /// <returns>The value returned by the command. Must not be null. (But possibly {null~Null})</returns>
        public abstract PValue Run(StackContext sctx, PValue[] args);

        #region Command groups

        private PCommandGroups _groups = PCommandGroups.None;

        /// <summary>
        /// A bit fields that represents memberships in the <see cref="PCommandGroups"/>.
        /// </summary>
        public PCommandGroups Groups
        {
            get { return _groups; }
        }

        /// <summary>
        /// Indicates whether the command belongs to a group.
        /// </summary>
        public bool BelongsToAGroup
        {
            get { return _groups != PCommandGroups.None; }
        }

        /// <summary>
        /// Determines whether the command is a member of a particular group.
        /// </summary>
        /// <param name="groups">The group (or groups) to test the command for.</param>
        /// <returns>True, if the command is a member of the supplied group (or all groups); false otherwise.</returns>
        public bool IsInGroup(PCommandGroups groups)
        {
            return (_groups & groups) == _groups;
            //If _groups contains groups, an AND operation won't alter it
        }

        /// <summary>
        /// Adds the command to the supplied group (or groups).
        /// </summary>
        /// <param name="additionalGroups">The group (or groups) to which to add the command.</param>
        public void AddToGroup(PCommandGroups additionalGroups)
        {
            _groups = _groups | additionalGroups;
        }

        /// <summary>
        /// Removes the command from the supplied group (or groups)
        /// </summary>
        /// <param name="groups">The group (or groups) from which to remove the command.</param>
        public void RemoveFromGroup(PCommandGroups groups)
        {
            _groups = _groups ^ (_groups & groups);
        }

        #endregion

        #region IIndirectCall Members

        /// <summary>
        /// Runs the command. (Calls <see cref="Run"/>)
        /// </summary>
        /// <param name="sctx">The stack context in which to call the command.</param>
        /// <param name="args">The arguments to pass to the command.</param>
        /// <returns>The value returned by the command.</returns>
        PValue IIndirectCall.IndirectCall(StackContext sctx, PValue[] args)
        {
            return Run(sctx, args) ?? PType.Null.CreatePValue();
        }

        #endregion
    }

    /// <summary>
    /// Defines command spaces (or groups).
    /// </summary>
    [Flags]
    public enum PCommandGroups
    {
        /// <summary>
        /// No command group.
        /// </summary>
        None = 0,
        /// <summary>
        /// The command group reserved for built-in commands. Supplied by the Prexonite VM.
        /// </summary>
        Engine = 1,
        /// <summary>
        /// The command group for commands provided by the host application.
        /// </summary>
        Host = 2,
        /// <summary>
        /// The command group for commands added by user code (script code).
        /// </summary>
        User = 4,
        /// <summary>
        /// The command group for commands added by the compiler (for the build block).
        /// </summary>
        Compiler = 8
    }
}