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
using Prexonite.Types;

namespace Prexonite
{
    /// <summary>
    /// This class represents an element on the runtime stack.
    /// </summary>
    /// <remarks>Please note that I might turn this class into the actual implementation of FunctionContext (by removing 'abstract' and sealing the class).</remarks>
    public abstract class StackContext : IIndirectCall
    {
        public Application ParentApplication
        {
            get { return Implementation.ParentApplication; }
        }

        #region Interface

        /// <summary>
        /// Represents the engine this context is part of.
        /// </summary>
        public abstract Engine ParentEngine
        {
            get;
        }

        /// <summary>
        /// Represents the function that provides the code and lexical environment for this stack context
        /// </summary>
        public abstract PFunction Implementation
        {
            get;
        }

        /// <summary>
        /// Indicates whether the context still has code/work to do.
        /// </summary>
        /// <returns>True if the context has additional work to perform in the next cycle, False if it has finished it's work and can be removed from the stack</returns>
        protected abstract bool PerformNextCylce();

        public abstract bool HandleException(Exception exc);

        internal bool NextCylce()
        {
            return PerformNextCylce();
        }

        /// <summary>
        /// Represents the return value of the context.
        /// Just providing a value here does not mean that it gets consumed by the caller.
        /// If the context does not provide a return value, this property should return null (not NullPType).
        /// </summary>
        public abstract PValue ReturnValue
        {
            get;
        }

        private ReturnModes _returnMode;

        public ReturnModes ReturnMode
        {
            get { return _returnMode; }
            set { _returnMode = value; }
        }

        #endregion

        #region Construct PType

        public PType ConstructPType(Type ptypeClrType, PValue[] args)
        {
            return ParentEngine.CreatePType(this, ptypeClrType, args);
        }

        public PType ConstructPType(ObjectPType ptypeClrType, PValue[] args)
        {
            return ParentEngine.CreatePType(this, ptypeClrType, args);
        }

        public PType ConstructPType(string typeName, PValue[] args)
        {
            return ParentEngine.CreatePType(this, typeName, args);
        }

        public PType ConstructPType(string expression)
        {
            return ParentEngine.CreatePType(this, expression);
        }

        #endregion

        #region Native PValue

        public PValue CreateNativePValue(object obj)
        {
            return ParentEngine.CreateNativePValue(obj);
        }

        #endregion

        #region IIndirectCall Members

        public PValue IndirectCall(StackContext sctx, PValue[] args)
        {
            sctx.ParentEngine.Process(this);
            return ReturnValue ?? PType.Null.CreatePValue();
        }

        #endregion
    }

    public enum ReturnModes
    {
        Exit,
        Break,
        Continue
    }
}