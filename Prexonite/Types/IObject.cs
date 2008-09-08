namespace Prexonite.Types
{
    public interface IObject
    {
        bool TryDynamicCall(
            StackContext sctx, PValue[] args, PCall call, string id, out PValue result);
    }
}