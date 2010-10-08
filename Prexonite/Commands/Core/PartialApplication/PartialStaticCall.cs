using Prexonite.Types;

namespace Prexonite.Commands.Core.PartialApplication
{
    public class PartialStaticCall : PartialApplicationBase
    {
        private readonly PType _ptype;
        private readonly string _memberId;
        private readonly PCall _call;

        public PartialStaticCall(int[] mappings, PValue[] closedArguments, PCall call, string memberId, PType ptype) : base(mappings, closedArguments, 0)
        {
            _ptype = ptype;
            _call = call;
            _memberId = memberId;
        }

        protected override PValue Invoke(StackContext sctx, PValue[] nonArguments, PValue[] arguments)
        {
            return _ptype.StaticCall(sctx, arguments, _call, _memberId);
        }
    }
}