using System;
using Prexonite.Types;

namespace Prexonite.Commands
{
    /// <summary>
    /// Command that calls <see cref="IDisposable.Dispose"/> on object values that support the interface.
    /// </summary>
    /// <remarks>
    /// Note that only wrapped .NET objects are disposed. Custom types that respond to "Dispose" are ignored.
    /// </remarks>
    public class Dispose : PCommand
    {
        /// <summary>
        /// Executes the dispose function.<br />
        /// Calls <see cref="IDisposable.Dispose"/> on object values that support the interface.
        /// </summary>
        /// <param name="sctx">The stack context. Ignored by this command.</param>
        /// <param name="args">The list of values to dispose.</param>
        /// <returns>Always null.</returns>
        /// <remarks><para>
        /// Note that only wrapped .NET objects are disposed. Custom types that respond to "Dispose" are ignored.</para>
        /// </remarks>
        public override PValue Run(StackContext sctx, PValue[] args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            foreach (PValue arg in args)
                if (arg != null && arg.Type is ObjectPType)
                {
                    IDisposable toDispose = arg.Value as IDisposable;
                    if (toDispose != null)
                        toDispose.Dispose();
                }
            return PType.Null.CreatePValue();
        }
    }
}