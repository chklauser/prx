using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prexonite.Compiler.Macro
{
    /// <summary>
    /// Interface for macro commands that also handle (some) partial applications.
    /// </summary>
    public abstract class PartialMacroCommand : MacroCommand
    {
        /// <summary>
        /// Creates a new partially applicable macro.
        /// </summary>
        /// <param name="id">The id of this macro</param>
        protected PartialMacroCommand(string id) : base(id)
        {
        }

        /// <summary>
        /// Implements the expansion of the partially applied macro. May refuse certain partial applications.
        /// </summary>
        /// <param name="context">The macro context for this macro expansion.</param>
        /// <returns>True, if the macro was successfully applied partially; false if partial application is illegal in this particular case.</returns>
        protected abstract bool DoExpandPartialApplication(MacroContext context);

        /// <summary>
        /// Attempts to expand the partial macro application.
        /// </summary>
        /// <param name="context">The macro context for this macro expansion.</param>
        /// <returns>True, if the macro was successfully applied partially; false if partial application is illegal in this particular case.</returns>
        public bool ExpandPartialApplication(MacroContext context)
        {
            return DoExpandPartialApplication(context);
        }
    }
}
