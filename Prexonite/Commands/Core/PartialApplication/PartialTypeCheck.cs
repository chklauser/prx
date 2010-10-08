using Prexonite.Types;

namespace Prexonite.Commands.Core.PartialApplication
{
    public class PartialTypeCheck : PartialApplicationBase
    {
        private readonly PType _ptype;

        public PartialTypeCheck(int[] mappings, PValue[] closedArguments, PType ptype) : base(mappings, closedArguments, 1)
        {
            _ptype = ptype;
        }

        protected override PValue Invoke(StackContext sctx, PValue[] nonArguments, PValue[] arguments)
        {
            return nonArguments[0].Type.Equals(_ptype);
        }
    }
}