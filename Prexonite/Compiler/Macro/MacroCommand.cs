using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Prexonite.Compiler.Ast;

namespace Prexonite.Compiler.Macro
{
    public abstract class MacroCommand
    {
        private readonly string _id;

        public MacroCommand(string id)
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
        /// <param name="args">The argument nodes to this macro expansion.</param>
        protected abstract void DoExpand(MacroContext context, IAstExpression[] args);

        public void Expand(MacroContext context, IAstExpression[] args)
        {
            DoExpand(context, args);
        }
    }
}
