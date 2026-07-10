using System;
using Prexonite;
using Prexonite.Types;

namespace Prx.Tests;

public class TestStackContext : StackContext
{
    public TestStackContext(Engine engine, Application app)
    {
        if (app == null)
            throw new ArgumentNullException(nameof(app));
        ParentEngine = engine ?? throw new ArgumentNullException(nameof(engine));
        Implementation = app.CreateFunction();
    }

    public override Engine ParentEngine { get; }

    public PFunction Implementation { get; }

    public override PValue ReturnValue => PType.Null.CreatePValue();

    /// <summary>
    ///     The parent application.
    /// </summary>
    public override Application ParentApplication => Implementation.ParentApplication;

    public override SymbolCollection ImportedNamespaces => Implementation.ImportedNamespaces;

    public override bool TryHandleException(Exception exc)
    {
        return false;
    }

    /// <summary>
    ///     Indicates whether the context still has code/work to do.
    /// </summary>
    /// <returns>True if the context has additional work to perform in the next cycle, False if it has finished it's work and can be removed from the stack</returns>
    protected override bool PerformNextCycle(StackContext? lastContext)
    {
        return false;
    }
}
