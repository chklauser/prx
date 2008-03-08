using System;
using System.Collections.Generic;
using System.Text;

namespace Prexonite.Compiler.Cil
{
    [Flags]
    public enum FunctionLinking
    {
        /// <summary>
        /// The CIL implementation itself is not available for static linking.
        /// </summary>
        Isolated = 0,

        /// <summary>
        /// The CIL implementation is available for static linking
        /// </summary>
        AvailableForLinking = 1,

        /// <summary>
        /// Function calls are linked statically whenever possible.
        /// </summary>
        Static = 2,

        /// <summary>
        /// Function calls are always linked by name.
        /// </summary>
        ByName = 0,

        /// <summary>
        /// The CIL implementation is completely independant of other implementations.
        /// </summary>
        FullyIsolated = Isolated | ByName,

        /// <summary>
        /// The CIL implementation supports static linking wherever possible.
        /// </summary>
        FullyStatic = AvailableForLinking | Static,

        /// <summary>
        /// The CIL implementation is isolated but links statically.
        /// </summary>
        JustStatic = Isolated | Static,

        /// <summary>
        /// The CIL implementation is available for static linking but links just by name.
        /// </summary>
        JustAvailableForLinking = AvailableForLinking | ByName
    }
}
