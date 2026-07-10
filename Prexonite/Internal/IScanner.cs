using Prexonite.Compiler;

namespace Prexonite.Internal;

interface IScanner
{
    Token Scan();
    Token Peek();
    void ResetPeek();
    string File { get; }
    void Abort();
}
