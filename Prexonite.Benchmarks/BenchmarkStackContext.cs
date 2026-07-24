using Prexonite.Types;

namespace Prexonite.Benchmarks;

internal sealed class BenchmarkStackContext : StackContext
{
    readonly PValue _returnValue = PType.Null.CreatePValue();

    internal BenchmarkStackContext(Engine engine, Application application)
    {
        ParentEngine = engine;
        ParentApplication = application;
        ImportedNamespaces = application.CreateFunction("benchmark-context").ImportedNamespaces;
    }

    public override Engine ParentEngine { get; }

    public override Application ParentApplication { get; }

    public override SymbolCollection ImportedNamespaces { get; }

    public override PValue ReturnValue => _returnValue;

    public override bool TryHandleException(Exception exc)
    {
        return false;
    }

    protected override bool PerformNextCycle(StackContext? lastContext)
    {
        return false;
    }
}
