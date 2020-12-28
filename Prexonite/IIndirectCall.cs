#nullable enable
using System;
using JetBrains.Annotations;

namespace Prexonite
{
    /// <summary>
    ///     Classes implementing this interface can react to indirect calls from Prexonite Script Code.
    /// </summary>
    /// <example>
    ///     <code>function main()
    ///         {
    ///         var obj = Get_an_object_that_implements_IIndirectCall();
    ///         obj.("argument"); //<see cref = "IndirectCall" /> will be called with the supplied argument.
    ///         }</code>
    /// </example>
    public interface IIndirectCall
    {
        /// <summary>
        ///     The reaction to an indirect call.
        /// </summary>
        /// <param name = "sctx">The stack context in which the object has been called indirectly.</param>
        /// <param name = "args">The array of arguments passed to the call.</param>
        /// <remarks>
        ///     <para>
        ///         Neither <paramref name = "sctx" /> nor <paramref name = "args" /> should be null. 
        ///         Implementations should raise an <see cref = "ArgumentNullException" /> when confronted with null as the StackContext.<br />
        ///         A null reference as the argument array should be silently converted to an empty array.
        ///     </para>
        ///     <para>
        ///         Implementations should <b>never</b> return null but instead return a <see cref = "PValue" /> object containing null.
        ///         <code>return Prexonite.Types.PType.Null.CreatePValue();</code>
        ///     </para>
        /// </remarks>
        /// <returns>The result of the call. Should <strong>never</strong> be null.</returns>
        [NotNull]
        PValue IndirectCall([NotNull] StackContext sctx, [NotNull] PValue[] args);
    }
}