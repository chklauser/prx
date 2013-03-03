// Prexonite
// 
// Copyright (c) 2013, Christian Klauser
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

namespace Prexonite.Commands
{
    /// <summary>
    ///     Implementation of <see cref = "PCommand" /> that forwards the run call to 
    ///     a class that implements <see cref = "ICommand" />.
    /// </summary>
    /// <seealso cref = "PCommand" />
    /// <seealso cref = "ICommand" />
    public sealed class NestedPCommand : PCommand
    {
        private readonly ICommand _action;

        /// <summary>
        ///     Provides access to the implementation of this specific instance of <see cref = "NestedPCommand" />.
        /// </summary>
        public ICommand Action
        {
            get { return _action; }
        }

        /// <summary>
        ///     Creates a new <see cref = "NestedPCommand" />.
        /// </summary>
        /// <param name = "action">Any implementation of <see cref = "ICommand" />.</param>
        /// <exception cref = "ArgumentNullException"><paramref name = "action" /> is null.</exception>
        public NestedPCommand(ICommand action)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            _action = action;
        }

        /// <summary>
        ///     Executes <see cref = "ICommand.Run" /> on <see cref = "Action" />.
        /// </summary>
        /// <param name = "sctx">The stack context in which to execute the command.</param>
        /// <param name = "args">The arguments to pass to the command invocation.</param>
        /// <returns>The value returned by <c><see cref = "Action" />.Run</c>.</returns>
        public override PValue Run(StackContext sctx, PValue[] args)
        {
            return _action.Run(sctx, args);
        }

        /// <summary>
        ///     Returns a description of the nested command instance.
        /// </summary>
        /// <returns>A description of the nested command instance.</returns>
        public override string ToString()
        {
            return "Nested(" + _action + ")";
        }
    }

    /// <summary>
    ///     Interface to be implemented by a class to be used as a command.
    /// </summary>
    /// <seealso cref = "PCommand" />
    /// <seealso cref = "NestedPCommand" />
    /// <remarks>
    ///     In order to be used as a command, <see cref = "NestedPCommand" /> need to be wrapped around instances of types that implement this interface.
    /// </remarks>
    public interface ICommand
    {
        /// <summary>
        ///     Actual implementation of a command.
        /// </summary>
        /// <param name = "sctx">The stack context in which the command is executed.</param>
        /// <param name = "args">The array of arguments supplied to the command.</param>
        /// <returns>The value returned by the command.</returns>
        /// <remarks>
        ///     If your implementation does not return a value, you have to return <c>PType.Null.CreatePValue()</c> and <strong>not</strong> <c>null</c>!
        /// </remarks>
        PValue Run(StackContext sctx, PValue[] args);
    }
}