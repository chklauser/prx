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
    public abstract class PCommand : IIndirectCall
    {
        private bool _isPure = false;

        public bool IsPure
        {
            get { return _isPure; }
            set { _isPure = value; }
        }

        public abstract PValue Run(StackContext sctx, PValue[] args);
        private PCommandGroups _groups = PCommandGroups.None;

        public PCommandGroups Groups
        {
            get { return _groups; }
        }

        public bool BelongsToAGroup
        {
            get { return _groups != PCommandGroups.None; }
        }

        public bool IsInGroup(PCommandGroups groups)
        {
            return (_groups & groups) == _groups;
                //If _groups contains groups, an AND operation won't alter it
        }

        public void AddToGroup(PCommandGroups additionalGroups)
        {
            _groups = _groups | additionalGroups;
        }

        public void RemoveFromGroup(PCommandGroups groups)
        {
            _groups = _groups ^ (_groups & groups);
        }

        #region IIndirectCall Members

        public PValue IndirectCall(StackContext sctx, PValue[] args)
        {
            return Run(sctx, args);
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