

namespace Prexonite.Compiler.Build;

public interface ISource
{
    bool CanOpen { get; }
    bool IsSingleUse { get; }
    bool TryOpen([NotNullWhen(true)] out TextReader? reader);
}