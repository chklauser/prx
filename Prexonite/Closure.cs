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

using System.Diagnostics;

namespace Prexonite;

/// <summary>
///     Represents a closure, a nested function bound to a set of shared variables.
/// </summary>
[DebuggerStepThrough]
public class Closure : IIndirectCall,
    IStackAware
{
    #region Properties

    /// <summary>
    ///     Provides readonly access to the function that makes up this closure.
    /// </summary>
    public PFunction Function { get; }

    /// <summary>
    ///     Provides readonly access to the list of variables the closure binds to the function.
    /// </summary>
    public PVariable[] SharedVariables { get; }

    #endregion

    #region Construction

    /// <summary>
    ///     Creates a new closure.
    /// </summary>
    /// <param name = "func">A (nested) function, that has shared variables.</param>
    /// <param name = "sharedVariables">A list of variables to share with the function.</param>
    /// <exception cref = "ArgumentNullException">Either <paramref name = "func" /> or <paramref name = "sharedVariables" /> is null.</exception>
    public Closure(PFunction func, PVariable[] sharedVariables)
    {
        Function = func ?? throw new ArgumentNullException(nameof(func));
        SharedVariables = sharedVariables ?? throw new ArgumentNullException(nameof(sharedVariables));
    }

    #endregion

    #region IIndirectCall Members

    /// <summary>
    ///     Invokes the function with the shared variables.
    /// </summary>
    /// <param name = "sctx">The stack context in which to invoke the function.</param>
    /// <param name = "args">A list of arguments to pass to the function.</param>
    /// <returns>The value returned by the function.</returns>
    public virtual PValue IndirectCall(StackContext sctx, PValue[] args)
    {
        var fctx = CreateStackContext(sctx, args);
        return sctx.ParentEngine.Process(fctx);
    }

    /// <summary>
    ///     Creates a stack context from the wrapped function.
    /// </summary>
    /// <param name = "sctx">The engine to bind to.</param>
    /// <param name = "args">A list of arguments to pass to the function.</param>
    /// <returns>A stack context for that function.</returns>
    public StackContext CreateStackContext(StackContext sctx, PValue[] args)
    {
        return CreateFunctionContext(sctx, args);
    }

    /// <summary>
    ///     Creates a function context from the wrapped function.
    /// </summary>
    /// <param name = "sctx">The stack context to bind to.</param>
    /// <param name = "args">A list of arguments to pass to the function.</param>
    /// <returns>A stack context for that function.</returns>
    /// <remarks>
    ///     Implementation may throw <see cref = "NotSupportedException" />.
    /// </remarks>
    /// <exception cref = "NotSupportedException">May be thrown by implementations</exception>
    public virtual FunctionContext CreateFunctionContext(StackContext sctx, PValue[] args)
    {
        return Function.CreateFunctionContext(sctx.ParentEngine, args, SharedVariables);
    }

    #endregion

    #region Equality

    /// <summary>
    ///     Determines whether two closures are equal.
    /// </summary>
    /// <param name = "a">A closure</param>
    /// <param name = "b">A closure</param>
    /// <returns>True, if the two closures use to the same function and the same shared variables; false otherwise.</returns>
    public static bool operator ==(Closure? a, Closure? b)
    {
        if ((object?) a == null && (object?) b == null)
            return true;
        else if ((object?) a == null || (object?) b == null)
            return false;
        else if (ReferenceEquals(a, b))
            return true;
        else
        {
            if (!ReferenceEquals(a.Function, b.Function))
                return false;
            if (a.SharedVariables.Length != b.SharedVariables.Length)
                return false;
            for (var i = 0; i < a.SharedVariables.Length; i++)
                if (!ReferenceEquals(a.SharedVariables[i], b.SharedVariables[i]))
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
    public static bool operator !=(Closure? a, Closure? b)
    {
        return !(a == b);
    }

    /// <summary>
    ///     Determines if the closure is equal to <paramref name = "obj" />.<br />
    ///     Closures can only be compared to other closures.
    /// </summary>
    /// <param name = "obj">Any object.</param>
    /// <returns>True if <paramref name = "obj" /> is a closure that is equal to the current instance.</returns>
    public override bool Equals(object? obj)
    {
        var clo = obj as Closure;
        if ((object?) clo == null)
            return false;
        return this == clo;
    }

    ///<summary>
    ///    Returns a hashcode.
    ///</summary>
    ///<returns>The function's hashcode.</returns>
    public override int GetHashCode()
    {
        return Function.GetHashCode();
    }

    /// <summary>
    ///     Returns a string that represents the closure.
    /// </summary>
    /// <returns>A string that represents the closure.</returns>
    public override string ToString()
    {
        return "Closure(" + Function + ")";
    }

    #endregion
}