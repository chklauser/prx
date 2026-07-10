using Prexonite.Commands.List;

namespace Prexonite;

public static class DependencyEntity<T>
{
    public static DependencyEntity<T, PValue> CreateDynamic(
        StackContext sctx,
        T name,
        PValue value,
        PValue getDependencies
    )
    {
        return new(name, value, _dynamicallyCallGetDependencies(sctx, getDependencies));
    }

    static Func<PValue, IEnumerable<T>>? _dynamicallyCallGetDependencies(
        StackContext sctx,
        PValue? getDependenciesPv
    )
    {
        if (getDependenciesPv == null)
            return null;

        return value =>
        {
            var depsPv = getDependenciesPv.IndirectCall(sctx, value);

            var depsDynamic = Map._ToEnumerable(sctx, depsPv);
            if (depsDynamic == null)
                throw new PrexoniteException("getDependencies function did not return enumerable.");

            return depsDynamic as IEnumerable<T>
                ?? (from pv in depsDynamic select pv.ConvertTo<T>(sctx, true));
        };
    }
}

public class DependencyEntity<TKey, TValue> : IDependent<TKey>
{
    readonly Func<TValue, IEnumerable<TKey>> _getDependencies;

    public DependencyEntity(
        TKey name,
        TValue value,
        Func<TValue, IEnumerable<TKey>>? getDependencies
    )
    {
        Name = name;
        _getDependencies =
            getDependencies ?? throw new ArgumentNullException(nameof(getDependencies));
        Value = value;
    }

    #region Implementation of INamed<TKey>

    public TKey Name { get; }

    #endregion

    public TValue Value { get; }

    #region Implementation of IDependent<TKey>

    public IEnumerable<TKey> GetDependencies()
    {
        return _getDependencies(Value);
    }

    #endregion
}
