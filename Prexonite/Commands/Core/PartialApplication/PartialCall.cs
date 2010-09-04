using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prexonite.Commands.Core.PartialApplication
{
    /// <summary>
    /// Represents a partial application of an indirect call (the default call interface in Prexonite)
    /// </summary>
    public class PartialCall : PartialApplicationBase
    {
        public PartialCall(sbyte[] mappings, PValue[] closedArguments) : base(mappings, closedArguments, 1)
        {
        }

        #region Overrides of PartialApplicationBase

        protected override PValue Invoke(StackContext sctx, PValue[] nonArguments, PValue[] arguments)
        {
            return nonArguments[0].IndirectCall(sctx, arguments);
        }

        #endregion
    }
}
