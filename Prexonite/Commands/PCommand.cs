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
///     The abstract base class for all commands (built-in functions)
/// </summary>
public abstract class PCommand : IIndirectCall
{
    /// <summary>
    ///     Executes the command.
    /// </summary>
    /// <param name = "sctx">The stack context in which to execut the command.</param>
    /// <param name = "args">The arguments to be passed to the command.</param>
    /// <returns>The value returned by the command. Must not be null. (But possibly {null~Null})</returns>
    public abstract PValue Run(StackContext sctx, PValue[] args);

    #region IIndirectCall Members

    /// <summary>
    ///     Runs the command. (Calls <see cref = "Run" />)
    /// </summary>
    /// <param name = "sctx">The stack context in which to call the command.</param>
    /// <param name = "args">The arguments to pass to the command.</param>
    /// <returns>The value returned by the command.</returns>
    PValue IIndirectCall.IndirectCall(StackContext sctx, PValue[] args)
    {
        return Run(sctx, args) ?? PType.Null.CreatePValue();
    }

    #endregion
}

/// <summary>
///     Defines command spaces (or groups).
/// </summary>
[Flags]
public enum PCommandGroups
{
    /// <summary>
    ///     No command group.
    /// </summary>
    None = 0,

    /// <summary>
    ///     The command group reserved for built-in commands. Supplied by the Prexonite VM.
    /// </summary>
    Engine = 1,

    /// <summary>
    ///     The command group for commands provided by the host application.
    /// </summary>
    Host = 2,

    /// <summary>
    ///     The command group for commands added by user code (script code).
    /// </summary>
    User = 4,

    /// <summary>
    ///     The command group for commands added by the compiler (for the build block).
    /// </summary>
    Compiler = 8,
}