// Prexonite
// 
// Copyright (c) 2014, Christian Klauser
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, 
//  are permitted provided that the following conditions are met:
// 
//     Redistributions of source code must retain the above copyright notice, 
//          this list of conditions and the following disclaimer.
//     Redistributions in binary form must reproduce the above copyright notice, 
//          this list of conditions and the following disclaimer in the 
//          documentation and/or other materials provided with the distribution.
//     The names of the contributors may be used to endorse or 
//          promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
//  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
//  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
//  IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
//  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
//  (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
//  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
//  WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING 
//  IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
using System;
using System.Diagnostics.CodeAnalysis;
using Prexonite.Types;

namespace Prexonite
{
    /// <summary>
    ///     This class represents an element on the runtime stack.
    /// </summary>
    public abstract class StackContext : IIndirectCall
    {
        #region Interface

        /// <summary>
        ///     Represents the engine this context is part of.
        /// </summary>
        public abstract Engine ParentEngine { get; }

        /// <summary>
        ///     The parent application.
        /// </summary>
        public abstract Application ParentApplication { get; }

        public abstract SymbolCollection ImportedNamespaces { get; }

        /// <summary>
        ///     Indicates whether the context still has code/work to do.
        /// </summary>
        /// <returns>True if the context has additional work to perform in the next cycle, False if it has finished it's work and can be removed from the stack</returns>
        protected abstract bool PerformNextCycle(StackContext lastContext);

        /// <summary>
        ///     Tries to handle the supplied exception.
        /// </summary>
        /// <param name = "exc">The exception to be handled.</param>
        /// <returns>True if the exception has been handled, false otherwise.</returns>
        public abstract bool TryHandleException(Exception exc);

        internal bool _NextCylce(StackContext lastContext)
        {
            return PerformNextCycle(lastContext);
        }

        /// <summary>
        ///     Represents the return value of the context.
        ///     Just providing a value here does not mean that it gets consumed by the caller.
        ///     If the context does not provide a return value, this property should return null (not NullPType).
        /// </summary>
        public abstract PValue ReturnValue { get; }

        /// <summary>
        ///     Gets or sets the mode of return.
        /// </summary>
        public ReturnMode ReturnMode { get; set; }

        public virtual CentralCache Cache => ParentApplication.Module.Cache;

        #endregion

        #region Construct PType

        /// <summary>
        ///     Constructs a PType instance from the supplied arguments.
        /// </summary>
        /// <param name = "ptypeClrType">A <see cref = "Type" /> of a class that inherits from <see cref = "PType" />.</param>
        /// <param name = "args">The list of arguments to pass to the constructor.</param>
        /// <returns>An instance of the supplied <see cref = "Type" />.</returns>
        public PType ConstructPType(Type ptypeClrType, PValue[] args)
        {
            return ParentEngine.CreatePType(this, ptypeClrType, args);
        }

        /// <summary>
        ///     Constructs a PType instance from the supplied arguments.
        /// </summary>
        /// <param name = "ptypeClrType">An <see cref = "ObjectPType" /> of a class that inherits from <see cref = "PType" />.</param>
        /// <param name = "args">The list of arguments to pass to the constructor.</param>
        /// <returns>An instance of the supplied <see cref = "Type" />.</returns>
        public PType ConstructPType(ObjectPType ptypeClrType, PValue[] args)
        {
            return ParentEngine.CreatePType(this, ptypeClrType, args);
        }

        /// <summary>
        ///     Constructs a PType instance from the supplied arguments.
        /// </summary>
        /// <param name = "typeName">The name of a class that inherits from <see cref = "PType" />.</param>
        /// <param name = "args">The list of arguments to pass to the constructor.</param>
        /// <returns>An instance of the supplied <see cref = "Type" />.</returns>
        public PType ConstructPType(string typeName, PValue[] args)
        {
            return ParentEngine.CreatePType(this, typeName, args);
        }

        /// <summary>
        ///     Constructs a PType instance from the supplied arguments.
        /// </summary>
        /// <param name = "expression">A constant type expression.</param>
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
        ///     Creates the native representation of an object in the Prexonite engine.
        /// </summary>
        /// <param name = "obj">The object to be represented.</param>
        /// <returns>A PValue containing the supplied object with an appropriate type.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames",
            MessageId = "obj")]
        public PValue CreateNativePValue(object obj)
        {
            return ParentEngine.CreateNativePValue(obj);
        }

        #endregion

        #region IIndirectCall Members

        /// <summary>
        ///     Executes the stack context and returns its return value.
        /// </summary>
        /// <param name = "sctx">The stack context in which to execute.</param>
        /// <param name = "args">ignored.</param>
        /// <returns>The value returned by the execution.</returns>
        PValue IIndirectCall.IndirectCall(StackContext sctx, PValue[] args)
        {
            return sctx.ParentEngine.Process(this);
        }

        #endregion
    }

    /// <summary>
    ///     The different modes of returning from a call.
    /// </summary>
    public enum ReturnMode
    {
        /// <summary>
        ///     The context has been exited normally. A return value may be available.
        /// </summary>
        Exit,

        /// <summary>
        ///     The context has been exited prematurely.
        /// </summary>
        Break,

        /// <summary>
        ///     The context has only been suspended.
        /// </summary>
        Continue
    }
}