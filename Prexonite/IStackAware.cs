namespace Prexonite
{
    public interface IStackAware
    {
        StackContext CreateStackContext(Engine eng, PValue[] args);
    }
}