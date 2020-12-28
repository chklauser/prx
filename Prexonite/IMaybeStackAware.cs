#nullable enable

using System.Diagnostics.CodeAnalysis;

namespace Prexonite
{
    /// <summary>
    ///     Partial application implementations commonly implement this interface. It allows
    ///     entities that are <see cref = "IStackAware" /> to retain this property even if 
    ///     partially applied.
    /// </summary>
    public interface IMaybeStackAware : IIndirectCall
    {
        /// <summary>
        ///     If the particular partial application supports it, create a 
        ///     stack context for executing the application. Otherwise it executes the application.
        /// </summary>
        /// <param name = "sctx">The caller's stack context.</param>
        /// <param name = "args">The arguments passed to the partial application by the caller.</param>
        /// <param name = "partialApplicationContext">If creation of stack context is successful, the stack context for executing 
        ///     the application. Otherwise undefined.</param>
        /// <param name = "result">If the creation of stack context is not successful, the return value of 
        ///     executing the application.</param>
        /// <returns>True if a stack context has been created; false if the application has been executed.</returns>
        bool TryDefer(StackContext sctx, PValue[] args,
           [NotNullWhen(true)] out StackContext partialApplicationContext,
           [NotNullWhen(false)] out PValue result);
    }
}