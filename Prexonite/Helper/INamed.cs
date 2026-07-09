
namespace Prexonite;

public interface INamed<out T>
{
    T Name { get; }
}