using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prexonite.Commands
{
    /// <summary>
    /// Describes capabilities and requirements of commands.
    /// </summary>
    /// <remarks>This type enables the CIL compiler to reason about commands that are not currently installed
    /// in the engine. Build commands and commands that are only available during macro expansions are 
    /// applications of this feature.</remarks>
    public interface ICommandInfo
    {
        /// <summary>
        /// Attempts to retrieve the CIL extension associated with this command.
        /// </summary>
        /// <param name="cilExtension">On success, holds a reference to the CIL extension. Undefined on failure.</param>
        /// <returns>True, on success; false on failure.</returns>
        bool TryGetCilExtension(out ICilExtension cilExtension);

        /// <summary>
        /// Attempts to retrieve the object describing the CIL compiler awareness of the command.
        /// </summary>
        /// <param name="cilCompilerAware">On success, holds a reference to the object describing the CIL compiler awareness of the command. Undefined on failure.</param>
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
                throw new ArgumentNullException("command");

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
            /// Attempts to retrieve the CIL extension associated with this command.
            /// </summary>
            /// <param name="cilExtension">On success, holds a reference to the CIL extension. Undefined on failure.</param>
            /// <returns>True, on success; false on failure.</returns>
            public bool TryGetCilExtension(out ICilExtension cilExtension)
            {
                cilExtension = _cilExtension;
                return cilExtension != null;
            }

            /// <summary>
            /// Attempts to retrieve the object describing the CIL compiler awareness of the command.
            /// </summary>
            /// <param name="cilCompilerAware">On success, holds a reference to the object describing the CIL compiler awareness of the command. Undefined on failure.</param>
            /// <returns>True, on success; false on failure</returns>
            public bool TryGetCilCompilerAware(out ICilCompilerAware cilCompilerAware)
            {
                cilCompilerAware = _cilCompilerAware;
                return cilCompilerAware != null;
            }

            #endregion
        }
    }
}
