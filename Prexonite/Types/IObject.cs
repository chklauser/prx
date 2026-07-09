

namespace Prexonite.Types;

public interface IObject
{
    bool TryDynamicCall(
        StackContext sctx,
        ReadOnlySpan<PValue> args,
        PCall call,
        string id,
        [NotNullWhen(true)]
        out PValue? result
    );
    
    // TODO add backwards compat overload
}