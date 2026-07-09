

using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.List;

public class HeadTail : PCommand, ICilCompilerAware
{
    #region Singleton

    HeadTail()
    {
    }

    public static HeadTail Instance { get; } = new();

    #endregion

    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args)
    {
        return RunStatically(sctx, args.ToArray());
    }

    public static PValue RunStatically(StackContext sctx, PValue[] args)
    {
        if (sctx == null)
            throw new ArgumentNullException(nameof(sctx));
        if (args == null)
            throw new ArgumentNullException(nameof(args));

        PValue? head;
        var nextArg = ((IEnumerable<PValue>) args).GetEnumerator();
        IEnumerator<PValue> nextX;
        try
        {
            if (!nextArg.MoveNext())
                throw new PrexoniteException("headtail requires at least one argument.");
            var arg = nextArg.Current;
            var xs = Map._ToEnumerable(sctx, arg);
            nextX = xs.GetEnumerator();
            try
            {
                if (!nextX.MoveNext())
                    return PType.Null;
                head = nextX.Current;
            }
            catch (Exception)
            {
                nextX.Dispose();
                throw;
            }
        }
        catch (Exception)
        {
            nextArg.Dispose();
            throw;
        }

        return
            (PValue)
            new List<PValue>
            {
                head,
                sctx.CreateNativePValue(
                    new Coroutine(new CoroutineContext(sctx, _tail(sctx, nextX, nextArg)))),
            };
    }

    static IEnumerable<PValue> _tail(StackContext sctx, IEnumerator<PValue> current,
        IEnumerator<PValue> remaining)
    {
        using (current)
            while (current.MoveNext())
                yield return current.Current;
        using (remaining)
        {
            while (remaining.MoveNext())
            {
                var xs = Map._ToEnumerable(sctx, remaining.Current);
                foreach (var x in xs)
                    yield return x;
            }
        }
    }

    public CompilationFlags CheckQualification(Instruction ins)
    {
        return CompilationFlags.PrefersRunStatically;
    }

    public void ImplementInCil(CompilerState state, Instruction ins)
    {
        throw new NotSupportedException();
    }
}