namespace Prexonite;

public interface IDependent<out T> : INamed<T>
{
    IEnumerable<T> GetDependencies();
}
