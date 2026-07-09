
#region Namespace Imports

#endregion

namespace Prexonite.Compiler.Cil;

[Flags]
public enum FunctionLinking
{
    /// <summary>
    ///     The CIL implementation itself is not available for static linking.
    /// </summary>
    Isolated = 0,

    /// <summary>
    ///     The CIL implementation is available for static linking
    /// </summary>
    AvailableForLinking = 1,

    /// <summary>
    ///     Function calls are always linked by name.
    /// </summary>
    ByName = 0,

    /// <summary>
    ///     Function calls are linked statically whenever possible.
    /// </summary>
    Static = 2,

    /// <summary>
    ///     The CIL implementation is completely independent of other implementations.
    /// </summary>
    FullyIsolated = Isolated | ByName,

    /// <summary>
    ///     The CIL implementation supports static linking wherever possible.
    /// </summary>
    FullyStatic = AvailableForLinking | Static,

    /// <summary>
    ///     The CIL implementation is isolated but links statically to functions available for linking.
    /// </summary>
    JustStatic = Isolated | Static,

    /// <summary>
    ///     The CIL implementation is available for static linking but links just by name.
    /// </summary>
    JustAvailableForLinking = AvailableForLinking | ByName,
}