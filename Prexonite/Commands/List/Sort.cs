namespace Prexonite.Commands.List;

/// <summary>
///     Implementation of the 'sort' command.
/// </summary>
public class Sort : PCommand
{
    #region Singleton pattern

    /// <summary>
    ///     As <see cref = "Sort" /> cannot be parametrized, Instance returns the one and only instance of the <see cref = "Sort" /> command.
    /// </summary>
    public static Sort Instance { get; } = new();

    Sort() { }

    #endregion

    /// <summary>
    ///     Sorts an IEnumerable.
    ///     <code>function sort(ref f1(a,b), ref f2(a,b), ... , xs)
    ///         { ... }</code>
    /// </summary>
    /// <param name = "sctx">The stack context in which the sort is performed.</param>
    /// <param name = "args">A list of sort expressions followed by the list to sort.</param>
    /// <returns>The a sorted copy of the list.</returns>
    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args)
    {
        if (sctx == null)
            throw new ArgumentNullException(nameof(sctx));
        var lst = new List<PValue>();
        if (args.Length == 0)
            return PType.Null.CreatePValue();
        else if (args.Length == 1)
        {
            var set = Map._ToEnumerable(sctx, args[0]);
            lst.AddRange(set);
            return (PValue)lst;
        }
        else
        {
            var clauses = new List<PValue>();
            for (var i = 0; i + 1 < args.Length; i++)
                clauses.Add(args[i]);
            lst.AddRange(Map._ToEnumerable(sctx, args[^1]));
            lst.Sort(
                delegate(PValue a, PValue b)
                {
                    foreach (var f in clauses)
                    {
                        var pdec = f.IndirectCall(sctx, a, b);
                        if (pdec.Type is not IntPType)
                            pdec = pdec.ConvertTo(sctx, PType.Int);
                        var dec = (int)pdec.Value!;
                        if (dec != 0)
                            return dec;
                    }
                    return 0;
                }
            );
            return (PValue)lst;
        }
    }

    //which might lead to initialization of the application.
}
