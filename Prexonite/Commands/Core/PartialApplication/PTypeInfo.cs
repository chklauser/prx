using Prexonite.Types;

namespace Prexonite.Commands.Core.PartialApplication
{
    /// <summary>
    /// Holds information about a PType at compile- and at run-time. Used in <see cref="PartialWithPTypeCommandBase{T}"/>.
    /// </summary>
    public class PTypeInfo
    {
        /// <summary>
        /// The runtime instance of the PType.
        /// </summary>
        public PType Type;

        /// <summary>
        /// The compile time constant PType expression.
        /// </summary>
        public string Expr;
    }
}