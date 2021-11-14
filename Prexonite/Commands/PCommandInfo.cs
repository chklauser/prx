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

namespace Prexonite.Commands;

/// <summary>
///     Describes capabilities and requirements of commands.
/// </summary>
/// <remarks>
///     This type enables the CIL compiler to reason about commands that are not currently installed
///     in the engine. Build commands and commands that are only available during macro expansions are 
///     applications of this feature.
/// </remarks>
public interface ICommandInfo
{
    /// <summary>
    ///     Attempts to retrieve the CIL extension associated with this command.
    /// </summary>
    /// <param name = "cilExtension">On success, holds a reference to the CIL extension. Undefined on failure.</param>
    /// <returns>True, on success; false on failure.</returns>
    bool TryGetCilExtension(out ICilExtension cilExtension);

    /// <summary>
    ///     Attempts to retrieve the object describing the CIL compiler awareness of the command.
    /// </summary>
    /// <param name = "cilCompilerAware">On success, holds a reference to the object describing the CIL compiler awareness of the command. Undefined on failure.</param>
    /// <returns>True, on success; false on failure</returns>
    bool TryGetCilCompilerAware(out ICilCompilerAware cilCompilerAware);
}

public static class CommandInfo
{
    private static ICommandInfo _toCommandInfo(object obj)
    {
        return new CachedCommandInfo(obj as ICilCompilerAware, obj as ICilExtension);
    }

    public static ICommandInfo ToCommandInfo(this PCommand command)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        return _toCommandInfo(command);
    }

    public static ICommandInfo ToCommandInfo(this ICilExtension cilExtension)
    {
        if (cilExtension == null)
            return new CachedCommandInfo(null, null);
        else
            return _toCommandInfo(cilExtension);
    }

    public static ICommandInfo ToCommandInfo(this ICilCompilerAware cilCompilerAware)
    {
        if (cilCompilerAware == null)
            return new CachedCommandInfo(null, null);
        else
            return _toCommandInfo(cilCompilerAware);
    }

    private sealed class CachedCommandInfo : ICommandInfo
    {
        private readonly ICilCompilerAware _cilCompilerAware;
        private readonly ICilExtension _cilExtension;

        public CachedCommandInfo(ICilCompilerAware cilCompilerAware, ICilExtension cilExtension)
        {
            _cilCompilerAware = cilCompilerAware;
            _cilExtension = cilExtension;
        }

        #region Implementation of ICommandInfo

        /// <summary>
        ///     Attempts to retrieve the CIL extension associated with this command.
        /// </summary>
        /// <param name = "cilExtension">On success, holds a reference to the CIL extension. Undefined on failure.</param>
        /// <returns>True, on success; false on failure.</returns>
        public bool TryGetCilExtension(out ICilExtension cilExtension)
        {
            cilExtension = _cilExtension;
            return cilExtension != null;
        }

        /// <summary>
        ///     Attempts to retrieve the object describing the CIL compiler awareness of the command.
        /// </summary>
        /// <param name = "cilCompilerAware">On success, holds a reference to the object describing the CIL compiler awareness of the command. Undefined on failure.</param>
        /// <returns>True, on success; false on failure</returns>
        public bool TryGetCilCompilerAware(out ICilCompilerAware cilCompilerAware)
        {
            cilCompilerAware = _cilCompilerAware;
            return cilCompilerAware != null;
        }

        #endregion
    }
}