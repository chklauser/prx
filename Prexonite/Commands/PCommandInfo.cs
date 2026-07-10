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
    bool TryGetCilExtension([NotNullWhen(true)] out ICilExtension? cilExtension);

    /// <summary>
    ///     Attempts to retrieve the object describing the CIL compiler awareness of the command.
    /// </summary>
    /// <param name = "cilCompilerAware">On success, holds a reference to the object describing the CIL compiler awareness of the command. Undefined on failure.</param>
    /// <returns>True, on success; false on failure</returns>
    bool TryGetCilCompilerAware([NotNullWhen(true)] out ICilCompilerAware? cilCompilerAware);
}

public static class CommandInfo
{
    static ICommandInfo _toCommandInfo(object obj)
    {
        return new CachedCommandInfo(obj as ICilCompilerAware, obj as ICilExtension);
    }

    extension(PCommand command)
    {
        public ICommandInfo ToCommandInfo()
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            return _toCommandInfo(command);
        }
    }

    extension(ICilExtension? cilExtension)
    {
        public ICommandInfo ToCommandInfo()
        {
            if (cilExtension == null)
                return new CachedCommandInfo(null, null);
            else
                return _toCommandInfo(cilExtension);
        }
    }

    extension(ICilCompilerAware? cilCompilerAware)
    {
        public ICommandInfo ToCommandInfo()
        {
            if (cilCompilerAware == null)
                return new CachedCommandInfo(null, null);
            else
                return _toCommandInfo(cilCompilerAware);
        }
    }

    sealed class CachedCommandInfo(ICilCompilerAware? compilerAware, ICilExtension? extension)
        : ICommandInfo
    {
        #region Implementation of ICommandInfo

        /// <summary>
        ///     Attempts to retrieve the CIL extension associated with this command.
        /// </summary>
        /// <param name = "cilExtension">On success, holds a reference to the CIL extension. Undefined on failure.</param>
        /// <returns>True, on success; false on failure.</returns>
        public bool TryGetCilExtension([NotNullWhen(true)] out ICilExtension? cilExtension)
        {
            cilExtension = extension;
            return cilExtension != null;
        }

        /// <summary>
        ///     Attempts to retrieve the object describing the CIL compiler awareness of the command.
        /// </summary>
        /// <param name = "cilCompilerAware">On success, holds a reference to the object describing the CIL compiler awareness of the command. Undefined on failure.</param>
        /// <returns>True, on success; false on failure</returns>
        public bool TryGetCilCompilerAware(
            [NotNullWhen(true)] out ICilCompilerAware? cilCompilerAware
        )
        {
            cilCompilerAware = compilerAware;
            return cilCompilerAware != null;
        }

        #endregion
    }
}
