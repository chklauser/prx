using System;
using System.Collections.Generic;
using System.Text;

namespace Prexonite.Commands
{
    /// <summary>
    /// A bit field that indicates how a subject integrates with cil compilation (compatibility etc.).
    /// </summary>
    [Flags]
    public enum CompilationFlags
    {
        /// <summary>
        /// Indicates that the subject is fully compatible with compilation to cil.
        /// </summary>
        IsCompatible = 0,

        /// <summary>
        /// Indicates that the subject cannot be used from compiled functions without special handling.
        /// </summary>
        IsIncompatible = 1,

        /// <summary>
        /// Indicates that the subject provides a custom compilation routine, invoked via <see cref="ICilCompilerAware.ImplementInCil"/>.
        /// </summary>
        HasCustomImplementation = 2,

        /// <summary>
        /// Indicates that the subject has a static member RunStatically(StackContext, PValue[]) the compiled function could statically bind to.
        /// </summary>
        HasRunStatically = 4,

        /// <summary>
        /// Indicates that the subject uses dynamic features and requires the caller to be interpreted.
        /// </summary>
        IsDynamic = 8,

        //Shortcuts
        /// <summary>
        /// Composed. Indicates that the subject is compatible but provides a static method for early binding.
        /// </summary>
        PreferRunStatically = IsCompatible | HasRunStatically,

        /// <summary>
        /// Composed. Indicates that the subject is compatible but provides a more efficient custom implementation via <see cref="ICilCompilerAware.ImplementInCil"/>.
        /// </summary>
        PreferCustomImplementation = IsCompatible | HasCustomImplementation,

        /// <summary>
        /// Composed. Indicates that the subject is not compatible but provides a workaround via <see cref="ICilCompilerAware.ImplementInCil"/>.
        /// </summary>
        HasCustomWorkaround = IsIncompatible | HasCustomImplementation,

        /// <summary>
        /// Composed. Indicates that the function uses dynamic features (requires an interpreted caller) but apart from that is compatible to cil compilation.
        /// </summary>
        OperatesOnCaller = IsCompatible | IsDynamic
    }
}
