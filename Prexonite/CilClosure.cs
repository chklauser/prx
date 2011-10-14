// Prexonite
// 
// Copyright (c) 2011, Christian Klauser
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
// 
//     Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//     Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//     The names of the contributors may be used to endorse or promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Diagnostics.CodeAnalysis;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

namespace Prexonite
{
    /// <summary>
    ///     Represents a closure, a nested function bound to a set of shared variables.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = "Cil")]
    public sealed class CilClosure : IIndirectCall, IStackAware
    {
        #region Properties

        private readonly PFunction _function;

        /// <summary>
        ///     Provides readonly access to the function that makes up this closure.
        /// </summary>
        public PFunction Function
        {
            get { return _function; }
        }

        private readonly PVariable[] _sharedVariables;

        /// <summary>
        ///     Provides readonly access to the list of variables the closure binds to the function.
        /// </summary>
        public PVariable[] SharedVariables
        {
            get { return _sharedVariables; }
        }

        #endregion

        #region Construction

        /// <summary>
        ///     Creates a new closure.
        /// </summary>
        /// <param name = "func">A (nested) function, that has shared variables.</param>
        /// <param name = "sharedVariables">A list of variables to share with the function.</param>
        /// <exception cref = "ArgumentNullException">Either <paramref name = "func" /> or <paramref name = "sharedVariables" /> is null.</exception>
        public CilClosure(PFunction func, PVariable[] sharedVariables)
        {
            if (func == null)
                throw new ArgumentNullException("func");
            if (sharedVariables == null)
                throw new ArgumentNullException("sharedVariables");

            if (!func.HasCilImplementation)
                throw new ArgumentException(func + " does not have a cil implemenetation");

            _function = func;
            _sharedVariables = sharedVariables;
        }

        #endregion

        #region IIndirectCall Members

        /// <summary>
        ///     Invokes the function with the shared variables.
        /// </summary>
        /// <param name = "sctx">The stack context in which to invoke the function.</param>
        /// <param name = "args">A list of arguments to pass to the function.</param>
        /// <returns>The value returned by the function.</returns>
        public PValue IndirectCall(StackContext sctx, PValue[] args)
        {
            if (!_function.HasCilImplementation)
                throw new PrexoniteException("CilClosure cannot handle " + _function +
                    " because it has no cil implementation");
            PValue result;
            ReturnMode returnMode;
            _function.CilImplementation(_function, sctx, args, _sharedVariables, out result,
                out returnMode);
            return result;
        }

        #endregion

        #region Equality

        /// <summary>
        ///     Determines whether two closures are equal.
        /// </summary>
        /// <param name = "a">A closure</param>
        /// <param name = "b">A closure</param>
        /// <returns>True, if the two closures use to the same function and the same shared variables; false otherwise.</returns>
        public static bool operator ==(CilClosure a, CilClosure b)
        {
            if ((object) a == null && (object) b == null)
                return true;
            else if ((object) a == null || (object) b == null)
                return false;
            else if (ReferenceEquals(a, b))
                return true;
            else
            {
                if (!ReferenceEquals(a._function, b._function))
                    return false;
                if (a._sharedVariables.Length != b._sharedVariables.Length)
                    return false;
                for (var i = 0; i < a._sharedVariables.Length; i++)
                    if (!ReferenceEquals(a._sharedVariables[i], b._sharedVariables[i]))
                        return false;
                return true;
            }
        }

        /// <summary>
        ///     Determines whether two closures are not equal.
        /// </summary>
        /// <param name = "a">A closure</param>
        /// <param name = "b">A closure</param>
        /// <returns>True, if the two closures do not use to the same function and the same shared variables; false otherwise.</returns>
        public static bool operator !=(CilClosure a, CilClosure b)
        {
            return !(a == b);
        }

        /// <summary>
        ///     Determines if the closure is equal to <paramref name = "obj" />.<br />
        ///     Closures can only be compared to other closures.
        /// </summary>
        /// <param name = "obj">Any object.</param>
        /// <returns>True if <paramref name = "obj" /> is a closure that is equal to the current instance.</returns>
        public override bool Equals(object obj)
        {
            var clo = obj as CilClosure;
            if (((object) clo) == null)
                return false;
            return this == clo;
        }

        ///<summary>
        ///    Returns a hashcode.
        ///</summary>
        ///<returns>The function's hashcode.</returns>
        public override int GetHashCode()
        {
            return _function.GetHashCode();
        }

        /// <summary>
        ///     Creates a stack context, that might later be pushed onto the stack.
        /// </summary>
        /// <param name = "sctx">The engine for which the context is to be created.</param>
        /// <param name = "args">The arguments passed to this instantiation.</param>
        /// <returns>The created <see cref = "StackContext" /></returns>
        public StackContext CreateStackContext(StackContext sctx, PValue[] args)
        {
            return _function.CreateFunctionContext(sctx.ParentEngine, args, _sharedVariables);
        }

        /// <summary>
        ///     Returns a string that represents the closure.
        /// </summary>
        /// <returns>A string that represents the closure.</returns>
        public override string ToString()
        {
            return "CilClosure(" + _function + ")";
        }

        #endregion
    }
}