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
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

namespace Prexonite
{
    /// <summary>
    /// Represents a closure, a nested function bound to a set of shared variables.
    /// </summary>
    [NoDebug]
    public class Closure : IIndirectCall,
                           IStackAware
    {
        #region Properties

        private PFunction _function;

        /// <summary>
        /// Provides readonly access to the function that makes up this closure.
        /// </summary>
        public PFunction Function
        {
            get { return _function; }
        }

        private PVariable[] _sharedVariables;

        /// <summary>
        /// Provides readonly access to the list of variables the closure binds to the function.
        /// </summary>
        public PVariable[] SharedVariables
        {
            get { return _sharedVariables; }
        }

        #endregion

        #region Construction

        /// <summary>
        /// Creates a new closure.
        /// </summary>
        /// <param name="func">A (nested) function, that has shared variables.</param>
        /// <param name="sharedVariables">A list of variables to share with the function.</param>
        /// <exception cref="ArgumentNullException">Either <paramref name="func"/> or <paramref name="sharedVariables"/> is null.</exception>
        public Closure(PFunction func, PVariable[] sharedVariables)
        {
            if (func == null)
                throw new ArgumentNullException("func");
            if (sharedVariables == null)
                throw new ArgumentNullException("sharedVariables");

            _function = func;
            _sharedVariables = sharedVariables;
        }

        #endregion

        #region IIndirectCall Members

        /// <summary>
        /// Invokes the function with the shared variables.
        /// </summary>
        /// <param name="sctx">The stack context in which to invoke the function.</param>
        /// <param name="args">A list of arguments to pass to the function.</param>
        /// <returns>The value returned by the function.</returns>
        public virtual PValue IndirectCall(StackContext sctx, PValue[] args)
        {
            StackContext fctx = CreateStackContext(sctx, args);
            return sctx.ParentEngine.Process(fctx);
        }

        /// <summary>
        /// Creates a stack context from the wrapped function.
        /// </summary>
        /// <param name="sctx">The engine to bind to.</param>
        /// <param name="args">A list of arguments to pass to the function.</param>
        /// <returns>A stack context for that function.</returns>
        public StackContext CreateStackContext(StackContext sctx, PValue[] args)
        {
            return CreateFunctionContext(sctx, args);
        }

        /// <summary>
        /// Creates a function context from the wrapped function.
        /// </summary>
        /// <param name="sctx">The stack context to bind to.</param>
        /// <param name="args">A list of arguments to pass to the function.</param>
        /// <returns>A stack context for that function.</returns>
        /// <remarks>Implementation may throw <see cref="NotSupportedException"/>.</remarks>
        /// <exception cref="NotSupportedException">May be thrown by implementations</exception>
        public virtual FunctionContext CreateFunctionContext(StackContext sctx, PValue[] args)
        {
            return _function.CreateFunctionContext(sctx.ParentEngine, args, _sharedVariables);
        }

        #endregion

        #region Equality

        /// <summary>
        /// Determines whether two closures are equal.
        /// </summary>
        /// <param name="a">A closure</param>
        /// <param name="b">A closure</param>
        /// <returns>True, if the two closures use to the same function and the same shared variables; false otherwise.</returns>
        public static bool operator ==(Closure a, Closure b)
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
                for (int i = 0; i < a._sharedVariables.Length; i++)
                    if (!ReferenceEquals(a._sharedVariables[i], b._sharedVariables[i]))
                        return false;
                return true;
            }
        }

        /// <summary>
        /// Determines whether two closures are not equal.
        /// </summary>
        /// <param name="a">A closure</param>
        /// <param name="b">A closure</param>
        /// <returns>True, if the two closures do not use to the same function and the same shared variables; false otherwise.</returns>
        public static bool operator !=(Closure a, Closure b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Determines if the closure is equal to <paramref name="obj"/>.<br />
        /// Closures can only be compared to other closures.
        /// </summary>
        /// <param name="obj">Any object.</param>
        /// <returns>True if <paramref name="obj"/> is a closure that is equal to the current instance.</returns>
        public override bool Equals(object obj)
        {
            Closure clo = obj as Closure;
            if (((object) clo) == null)
                return false;
            return this == clo;
        }

        ///<summary>
        /// Returns a hashcode.
        ///</summary>
        ///<returns>The function's hashcode.</returns>
        public override int GetHashCode()
        {
            return _function.GetHashCode();
        }

        /// <summary>
        /// Returns a string that represents the closure.
        /// </summary>
        /// <returns>A string that represents the closure.</returns>
        public override string ToString()
        {
            return "Closure(" + _function.Id + ")";
        }

        #endregion
    }
}