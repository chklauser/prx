using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Prexonite.Types;

namespace Prexonite.Commands.Core.PartialApplication
{
    /// <summary>
    /// Partial application of object creation calls.
    /// </summary>
    public class PartialConstruction : PartialApplicationBase
    {
        private readonly PType _type;

        /// <summary>
        /// Creates a new partial construction instance.
        /// </summary>
        /// <param name="mappings">The argument mappings for this partial application.</param>
        /// <param name="closedArguments">The closed arguments referred to in <paramref name="mappings"/>.</param>
        /// <param name="type">The <see cref="PType"/> to construct instances of.</param>
        public PartialConstruction(int[] mappings, PValue[] closedArguments, PType type) : base(mappings, closedArguments, 0)
        {
            _type = type;
        }

        #region Overrides of PartialApplicationBase

        protected override PValue Invoke(StackContext sctx, PValue[] nonArguments, PValue[] arguments)
        {
            return _type.Construct(sctx, arguments);
        }

        #endregion
    }
}
