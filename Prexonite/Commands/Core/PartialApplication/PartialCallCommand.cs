using System;
using System.Reflection;

namespace Prexonite.Commands.Core.PartialApplication
{
    /// <summary>
    /// <para>Used to implement partial application for indirect calls (the default call interface in Prexonite)</para>
    /// </summary>
    public class PartialCallCommand : PartialApplicationCommandBase<Object>
    {
        #region Overrides of PartialApplicationCommandBase<object>

        protected override IIndirectCall CreatePartialApplication(StackContext sctx, int[] mappings, PValue[] closedArguments, object parameter)
        {
            return new PartialCall(mappings, closedArguments);
        }

        protected override Type GetPartialCallRepresentationType(object parameter)
        {
            return typeof (PartialCall);
        }

        #endregion
    }
}