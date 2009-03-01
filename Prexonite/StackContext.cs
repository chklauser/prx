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

        #region Interface

        /// <summary>
        /// Represents the engine this context is part of.
        /// </summary>
        public abstract Engine ParentEngine
        {
            get;
        }

        /// <summary>
        /// The parent application.
        /// </summary>
        public abstract Application ParentApplication
        {
            get;
        }

        public abstract SymbolCollection ImportedNamespaces
        {
            get;
        }

        /// <summary>
        /// Indicates whether the context still has code/work to do.
        /// </summary>
        /// <returns>True if the context has additional work to perform in the next cycle, False if it has finished it's work and can be removed from the stack</returns>
        protected abstract bool PerformNextCylce(StackContext lastContext);

        /// <summary>
        /// Tries to handle the supplied exception.
        /// </summary>
        /// <param name="exc">The exception to be handled.</param>
        /// <returns>True if the exception has been handled, false otherwise.</returns>
        public abstract bool TryHandleException(Exception exc);

        internal bool NextCylce(StackContext lastContext)
        {
            return PerformNextCylce(lastContext);
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

        /// <summary>
        /// Gets or sets the mode of return.
        /// </summary>
        public ReturnModes ReturnMode { get; set; }

        #endregion

        #region Construct PType

        /// <summary>
        /// Constructs a PType instance from the supplied arguments.
        /// </summary>
        /// <param name="ptypeClrType">A <see cref="Type"/> of a class that inherits from <see cref="PType"/>.</param>
        /// <param name="args">The list of arguments to pass to the constructor.</param>
        /// <returns>An instance of the supplied <see cref="Type"/>.</returns>
        public PType ConstructPType(Type ptypeClrType, PValue[] args)
        {
            return ParentEngine.CreatePType(this, ptypeClrType, args);
        }

        /// <summary>
        /// Constructs a PType instance from the supplied arguments.
        /// </summary>
        /// <param name="ptypeClrType">An <see cref="ObjectPType"/> of a class that inherits from <see cref="PType"/>.</param>
        /// <param name="args">The list of arguments to pass to the constructor.</param>
        /// <returns>An instance of the supplied <see cref="Type"/>.</returns>
        public PType ConstructPType(ObjectPType ptypeClrType, PValue[] args)
        {
            return ParentEngine.CreatePType(this, ptypeClrType, args);
        }

        /// <summary>
        /// Constructs a PType instance from the supplied arguments.
        /// </summary>
        /// <param name="typeName">The name of a class that inherits from <see cref="PType"/>.</param>
        /// <param name="args">The list of arguments to pass to the constructor.</param>
        /// <returns>An instance of the supplied <see cref="Type"/>.</returns>
        public PType ConstructPType(string typeName, PValue[] args)
        {
            return ParentEngine.CreatePType(this, typeName, args);
        }

        /// <summary>
        /// Constructs a PType instance from the supplied arguments.
        /// </summary>
        /// <param name="expression">A constant type expression.</param>
        /// <returns>A PType based on the supplied type expression.</returns>
        /// <remarks>
        ///     <para>Note that the function does not accept Prexonite type expression.</para>
        ///     <para>Type expressions use parenthesis instead of angle brackets.</para>
        /// </remarks>
        public PType ConstructPType(string expression)
        {
            return ParentEngine.CreatePType(this, expression);
        }

        #endregion

        #region Native PValue

        /// <summary>
        /// Creates the native representation of an object in the Prexonite engine.
        /// </summary>
        /// <param name="obj">The object to be represented.</param>
        /// <returns>A PValue containing the supplied object with an appropriate type.</returns>
        public PValue CreateNativePValue(object obj)
        {
            return ParentEngine.CreateNativePValue(obj);
        }

        #endregion

        #region IIndirectCall Members

        /// <summary>
        /// Executes the stack context and returns its return value.
        /// </summary>
        /// <param name="sctx">The stack context in which to execute.</param>
        /// <param name="args">ignored.</param>
        /// <returns>The value returned by the execution.</returns>
        PValue IIndirectCall.IndirectCall(StackContext sctx, PValue[] args)
        {
            return sctx.ParentEngine.Process(this);
        }

        #endregion
    }

    /// <summary>
    /// The different modes of returning from a call.
    /// </summary>
    public enum ReturnModes
    {
        /// <summary>
        /// The context has been exited normally. A return value may be available.
        /// </summary>
        Exit,

        /// <summary>
        /// The context has been exited prematurely.
        /// </summary>
        Break,

        /// <summary>
        /// The context has only been suspended.
        /// </summary>
        Continue
    }
}