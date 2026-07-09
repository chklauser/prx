

namespace Prexonite.Modular;

public interface IResourceDescriptor
{
    Stream Open();
    Task ExtractAsync(string destinationPath);
    void Extract(string destinationPath);
}