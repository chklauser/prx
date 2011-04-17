using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Prexonite.Compiler.Ast;

namespace Prexonite.Compiler.Macro
{
    /// <summary>
    /// Interface for commands that are applied at compile-time.
    /// </summary>
    public abstract class MacroCommand
    {
        private readonly string _id;

        /// <summary>
        /// Creates a new instance of the macro command. It will identify itself with the supplied id.
        /// </summary>
        /// <param name="id">The name of the physical slot, this command resides in.</param>
        protected MacroCommand(string id)
        {
            if (String.IsNullOrWhiteSpace(id))
                throw new ArgumentException("MacroCommad.Id must not be null or empty.");
            _id = id;
        }
        
        /// <summary>
        /// ID (slot name) of this macro command.
        /// </summary>
        public string Id
        {
            [DebuggerStepThrough]
            get { return _id; }
        }

        /// <summary>
        /// Implementation of the application of this macro.
        /// </summary>
        /// <param name="context">The macro context for this macro expansion.</param>
        protected abstract void DoExpand(MacroContext context);

        public void Expand(MacroContext context)
        {
            DoExpand(context);
        }
    }
}
