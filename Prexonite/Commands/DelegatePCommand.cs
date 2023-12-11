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

namespace Prexonite.Commands;

/// <summary>
///     Implementation of <see cref = "PCommand" /> using delegates.
/// </summary>
/// <seealso cref = "PCommand" />
/// <seealso cref = "PCommandAction" />
public sealed class DelegatePCommand : PCommand
{
    /// <summary>
    ///     Provides readonly access to the delegate used to implement the current instance of <see cref = "DelegatePCommand" />.
    /// </summary>
    public PCommandAction Action { get; }

    /// <summary>
    ///     Returns a string that describes the current instance of <see cref = "DelegatePCommand" />.
    /// </summary>
    /// <returns>A string that describes the current instance of <see cref = "DelegatePCommand" /></returns>
    public override string ToString()
    {
        return "Delegate(" + Action + ")";
    }

    /// <summary>
    ///     Forwards the call to the actual implementation, the delegate <see cref = "Action" />.
    /// </summary>
    /// <param name = "sctx">The stack context in which to execute the command.</param>
    /// <param name = "args">The array of arguments to pass to the command.</param>
    /// <returns></returns>
    public override PValue Run(StackContext sctx, PValue[] args)
    {
        return Action(sctx, args);
    }

    /// <summary>
    ///     Creates a new <see cref = "DelegatePCommand" />.
    /// </summary>
    /// <param name = "action">An implementation of the <see cref = "PCommand.Run" /> method.</param>
    /// <exception cref = "ArgumentNullException"><paramref name = "action" /> is null.</exception>
    public DelegatePCommand(PCommandAction action)
        : this(action, false)
    {
    }

    /// <summary>
    ///     Creates a new <see cref = "DelegatePCommand" />.
    /// </summary>
    /// <param name = "action">An implementation of the <see cref = "PCommand.Run" /> method.</param>
    /// <param name = "isPure">A boolean value indicating whether the command is to be treated like a pure function.</param>
    /// <exception cref = "ArgumentNullException"><paramref name = "action" /> is null.</exception>
    public DelegatePCommand(PCommandAction action, bool isPure)
    {
        Action = action ?? throw new ArgumentNullException(nameof(action));
    }

    /// <summary>
    ///     Syntactic sugar for the creation of commands.
    /// </summary>
    /// <param name = "action">An implementation of the <see cref = "PCommand.Run" /> method.</param>
    /// <returns>A new instance of <see cref = "DelegatePCommand" />.</returns>
    /// <exception cref = "ArgumentNullException"><paramref name = "action" /> is null.</exception>
    public static implicit operator DelegatePCommand(PCommandAction action)
    {
        return new(action);
    }
}

/// <summary>
///     Emulates <see cref = "PCommand.Run" /> for use in <see cref = "DelegatePCommand" />.
/// </summary>
/// <param name = "sctx">The stack context in which the command is executed.</param>
/// <param name = "arguments">The array of arguments passed to the command invocation.</param>
/// <returns>The value returned by the command.</returns>
/// <remarks>
///     If your implementation does not return a value, you have to return <c>PType.Null.CreatePValue()</c> and <strong>not</strong> <c>null</c>!
/// </remarks>
/// <seealso cref = "DelegatePCommand" />
/// <seealso cref = "PCommand" />
public delegate PValue PCommandAction(StackContext sctx, PValue[] arguments);