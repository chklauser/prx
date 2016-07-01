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
using System.Collections.Generic;
using System.Diagnostics;
using Prexonite.Modular;
using Prexonite.Types;

namespace Prexonite
{
    /// <summary>
    ///     An instance of a <see cref="VariableDeclaration"/>, represents an "address" in the Prexonite VM.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         PVariables are used for both local and global variables with the difference that 
    ///         local variables don't usually have a <see cref = "Meta">MetaTable</see>.
    ///     </para>
    ///     <para>
    ///         Also, variable references are just references to the PVariable objects applications or functions.
    ///     </para>
    /// </remarks>
    [DebuggerStepThrough]
    public sealed class PVariable : IHasMetaTable,
                                    IIndirectCall
    {
        private PValue _value = PType.Null;
        //Variable metatables are only created when requested.
        private VariableDeclaration _declaration;

        /// <summary>
        /// Returns the id of this variable. For local variables, this id might be different from the
        /// physical name of that variable.
        /// </summary>
        public string Id
        {
            get { return Meta[Application.IdKey]; }
        }

        /// <summary>
        /// The variable declaration this variable instance is based on.
        /// </summary>
        public VariableDeclaration Declaration
        {
            get
            {
                if (_declaration == null)
                    _declaration = VariableDeclaration.Create(Engine.GenerateName());
                return _declaration;
            }
        }

        //Meta and IHasMetaTable are on PVariable for historic reasons.
        /// <summary>
        ///     Provides readonly access to the variable's <see cref = "MetaTable" />.
        /// </summary>
        /// <remarks>
        ///     The default constructor does not create a <see cref = "MetaTable" /> but as soon as this property is accessed for the first time, one will be instantiated.
        /// </remarks>
        public MetaTable Meta
        {
            get
            {
                return Declaration.Meta;
            }
        }

        /// <summary>
        ///     Provides access to the value stored in the variable.
        /// </summary>
        /// <value>The PValue object stored in this variables or a PValue(null) object if the reference is null.</value>
        public PValue Value
        {
            get { return _value; }
            set
            {
                Debug.Assert(value != null);
                _value = value;
            }
        }

        #region Construction

        /// <summary>
        ///     Creates a new (local) variable.
        /// </summary>
        /// <remarks>
        ///     This constructor does not create a <see cref = "MetaTable" />.
        /// </remarks>
        public PVariable()
        {
        }

        /// <summary>
        ///     Creates a new (global) variable.
        /// </summary>
        /// <param name = "name">An identifier to be stored in the variable's <see cref = "Meta">MetaTable</see>.</param>
        /// <remarks>
        ///     This constructor creates a <see cref = "MetaTable" />.
        /// </remarks>
        /// <exception cref = "ArgumentNullException"><paramref name = "name" /> is null.</exception>
        /// <exception cref = "ArgumentException"><paramref name = "name" /> is empty.</exception>
        public PVariable(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (name.Length == 0)
                throw new ArgumentException("name is expected to contain at least one character.");
            _declaration = VariableDeclaration.Create(name);
        }

        /// <summary>
        /// Creates a new variable instance based on the provided variable declaration.
        /// </summary>
        /// <param name="variableDeclaration">The variable declaration this variable is based on.</param>
        public PVariable(VariableDeclaration variableDeclaration)
        {
            if (variableDeclaration == null)
                throw new ArgumentNullException(nameof(variableDeclaration));
            _declaration = variableDeclaration;
        }

        #endregion

        #region IIndirectCall Members

        /// <summary>
        ///     "Calling" a variable means "getting" or "setting" the variable's value.
        /// </summary>
        /// <param name = "sctx">The stack context in which to perform the indirect call.</param>
        /// <param name = "args">The list of arguments to be passed to this indirect call. <br />
        ///     Only the first element of the array is ever used in by this special implementation of <see
        ///      cref = "IIndirectCall.IndirectCall" />.</param>
        /// <remarks>
        ///     If <paramref name = "args" /> is empty, just the variable's current value is returned.<br />
        ///     Otherwise, the first element of <paramref name = "args" /> is assigned to the variable.
        /// </remarks>
        /// <returns>Always the variable's (new) value.</returns>
        public PValue IndirectCall(StackContext sctx, PValue[] args)
        {
            if (args.Length != 0)
                _value = args[args.Length - 1];
            return _value;
        }

        #endregion
    }
}