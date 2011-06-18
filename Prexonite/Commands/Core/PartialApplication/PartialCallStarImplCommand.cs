using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prexonite.Commands.Core.PartialApplication
{
    public class PartialCallStarImplCommand : PartialApplicationCommandBase<Object>
    {

        #region Singleton pattern

        private static readonly PartialCallStarImplCommand _instance = new PartialCallStarImplCommand();

        public static PartialCallStarImplCommand Instance
        {
            get { return _instance; }
        }

        private PartialCallStarImplCommand()
        {
        }

        #endregion

        public const string Alias = @"pa\call\star";

        #region Overrides of PartialApplicationCommandBase<object>

        protected override IIndirectCall CreatePartialApplication(StackContext sctx, int[] mappings, PValue[] closedArguments, object parameter)
        {
            return new PartialCallStar(mappings, closedArguments);
        }

        protected override Type GetPartialCallRepresentationType(object parameter)
        {
            return typeof (PartialCallStar);
        }

        #endregion
    }
}
