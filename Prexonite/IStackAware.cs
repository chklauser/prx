namespace Prexonite
{
    /// <summary>
    /// Marks objects that can create a representation/instance of themselves to be put on the stack.
    /// </summary>
    public interface IStackAware
    {
        /// <summary>
        /// Creates a stack context, that might later be pushed onto the stack.
        /// </summary>
        /// <param name="sctx">The engine for which the context is to be created.</param>
        /// <param name="args">The arguments passed to this instantiation.</param>
        /// <returns>The created <see cref="StackContext"/></returns>
        StackContext CreateStackContext(StackContext sctx, PValue[] args);
    }
}