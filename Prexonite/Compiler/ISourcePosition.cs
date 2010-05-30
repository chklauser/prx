using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prexonite.Compiler
{
    /// <summary>
    /// Provides source position information.
    /// </summary>
    public interface ISourcePosition
    {
        /// <summary>
        /// The source file that declared this object.
        /// </summary>
        string File { get; }

        /// <summary>
        /// The line in the source file that declared this object.
        /// </summary>
        int Line { get; }

        /// <summary>
        /// The column in the source file that declared this object.
        /// </summary>
        int Column { get; }
    }
}
